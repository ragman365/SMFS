using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraPrinting;
using DevExpress.Utils;
using GeneralLib;
using DevExpress.XtraGrid.Views.Base;
using System.Linq;
using DevExpress.CodeParser;
using System.Configuration;
using DevExpress.XtraReports.Wizards;
using System.IO;
using iTextSharp.text.pdf;
using System.Text.RegularExpressions;
/****************************************************************************************/
namespace SMFS
{
    /****************************************************************************************/
    public partial class FastLookup : DevExpress.XtraEditors.XtraForm
    {
        /****************************************************************************************/
        private bool fromFuneral = false;
        private string workContract = "";
        private string workFirstName = "";
        private string workLastName = "";
        public FastLookup(string contract = "", bool funeral = false )
        {
            InitializeComponent();
            workContract = contract;
            fromFuneral = funeral;
        }
        /****************************************************************************************/
        public FastLookup(string firstName, string lastName )
        {
            InitializeComponent();
            workContract = "";
            fromFuneral = false;
            workFirstName = firstName;
            workLastName = lastName;
        }
        /****************************************************************************************/
        private void FastLookup_Load(object sender, EventArgs e)
        {
            btnPreneed.Hide();
            chkFilter.Hide();
            chkPayersOnly.Hide();
            dgv.Hide();
            dgv2.Hide();
            dgv.Dock = DockStyle.Fill;
            dgv2.Dock = DockStyle.Fill;
            if (ListDone == null)
                btnSelect.Hide();
            if (LoginForm.username.ToUpper() == "ROBBY")
                button1.Show();
            else
                button1.Hide();
            txtContract.Text = workContract;
            chkFilter.Checked = true;
            if ( G1.isField() )
            {
                //groupBox1.Hide();
                chkFilter.Hide();
                chkPayersOnly.Hide();
                chkHonor.Hide();
                chkSecNat.Hide();
                chkThirdOnly.Hide();
                customerPayoffAsOf10DaysToolStripMenuItem.Visible = false;
                deathPayoffAsOfTodayToolStripMenuItem.Visible = false;
            }
            if ( !String.IsNullOrWhiteSpace ( workFirstName) && !String.IsNullOrWhiteSpace ( workLastName ))
            {
                txtLastName.Text = workLastName;
                txtFirstName.Text = workFirstName;
                btnRun_Click(null, null);
            }
        }
        /****************************************************************************************/
        private void btnRun_Click(object sender, EventArgs e)
        {
            try
            {
                string id = txtID.Text.Trim();
                if ( !String.IsNullOrWhiteSpace ( id))
                {
                    SearchID(id);
                    return;
                }
                if (radioTrusts.Checked)
                    SearchTrusts();
                else if (radioFunerals.Checked)
                    SearchTrusts( true );
                else if (radioInsurance.Checked)
                    SearchInsurance();
                else if (radioPayers.Checked)
                    SearchPayers();
                else
                    SearchPolicies();
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** Problem Locating Customer! " + ex.Message.ToString());
            }
        }
        /****************************************************************************************/
        private void SearchID( string id )
        {
            string cmd = "Select * from `payments` where `s50` = '" + id + "';";
            DataTable dx = G1.get_db_data(cmd);
            if ( dx.Rows.Count <= 0 )
            {
                cmd = "Select * from `ipayments` where `s50` = '" + id + "';";
                dx = G1.get_db_data(cmd);
            }
            if ( dx.Rows.Count <= 0 )
            {
                MessageBox.Show("***ERROR*** Problem Locating Customer Payment ID!");
                return;
            }
            string contractNumber = dx.Rows[0]["contractNumber"].ObjToString();

            this.Cursor = Cursors.WaitCursor;
            CustomerDetails clientForm = new CustomerDetails(contractNumber);
            clientForm.Show();
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void SearchPolicies()
        {
            dgv.Visible = false;
            string contract = this.txtContract.Text;
            string lastName = this.txtLastName.Text;
            string firstName = this.txtFirstName.Text;
            string address = this.txtAddress.Text;
            string ssn = this.txtSSN.Text;

            DataTable dt = null;
            DataTable dx = null;
            string cmd = "";
            string str = "";

            string contractFile = "icontracts";
            string customerFile = "icustomers";
            string policies = "policies";

            if (!String.IsNullOrWhiteSpace(contract))
            {
                cmd = "Select * from `" + customerFile + "` c JOIN `" + contractFile + "` m ON c.`contractNumber` = m.`contractNumber` where c.`contractNumber` = 'XYZZY';";
                dt = G1.get_db_data(cmd);

                cmd = "Select * from `" + customerFile + "` c JOIN `" + contractFile + "` m ON c.`contractNumber` = m.`contractNumber` JOIN `policies` p ON c.`payer` = p.`payer` where ";
                cmd += " p.`policyNumber` = '" + contract + "';";
                dt = G1.get_db_data(cmd);
                if ( dt.Rows.Count <= 0 )
                {
                    cmd = "Select * from `policies` p where ";
                    cmd += " p.`payer` = '" + contract + "';";
                    dt = G1.get_db_data(cmd);
                    if ( dt.Rows.Count > 0 )
                    {
                        contract = dt.Rows[0]["contractNumber"].ObjToString();
                        cmd = "Select * from `" + customerFile + "` c JOIN `" + contractFile + "` m ON c.`contractNumber` = m.`contractNumber` JOIN `policies` p ON c.`payer` = p.`payer` where ";
                        cmd += " m.`contractNumber` = '" + contract + "';";
                        dt = G1.get_db_data(cmd);
                    }
                    else
                    {
                        dt.Rows.Clear();
                    }
                }
                if (dt.Rows.Count <= 0)
                {
                    MessageBox.Show("***ERROR *** Cannot locate any Contract or Payer using Policy Number " + contract + "!", "Policy Lookup Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                    return;
                }
            }
            else
            {

                bool found = false;
                this.Cursor = Cursors.WaitCursor;
                cmd = "Select * from `icustomers` c JOIN `icontracts` m ON c.`contractNumber` = m.`contractNumber` where c.`contractNumber` = 'XYZZY';";
                dt = G1.get_db_data(cmd);

                dgv.DataSource = dt;

                firstName = firstName.Replace("*", "%");
                lastName = lastName.Replace("*", "%");
                address = address.Replace("*", "%");
                ssn = ssn.Replace("*", "%");
                ssn = ssn.Replace("-", "");
                cmd = "Select * from `" + customerFile + "` c JOIN `" + contractFile + "` m ON c.`contractNumber` = m.`contractNumber` JOIN `policies` p ON c.`payer` = p.`payer` where ";
                if (!String.IsNullOrWhiteSpace(firstName))
                {
                    found = true;
                    if (firstName.IndexOf("%") >= 0)
                        cmd += " p.`policyFirstName` LIKE '" + firstName + "' ";
                    else
                        cmd += " p.`policyFirstName` = '" + firstName + "' ";
                }
                if (!String.IsNullOrWhiteSpace(lastName))
                {
                    if (found)
                        cmd += " AND ";
                    found = true;
                    if (lastName.IndexOf("%") >= 0)
                        cmd += " p.`policyLastName` LIKE '" + lastName + "' ";
                    else
                        cmd += " p.`policyLastName` = '" + lastName + "' ";
                }
                if (!String.IsNullOrWhiteSpace(address))
                {
                    if (found)
                        cmd += " AND ";
                    found = true;
                    if (address.IndexOf("%") >= 0)
                        cmd += " CONCAT_WS('', `address1`, `city`, `state`, `zip1` ) LIKE '" + address + "' ";
                    else
                        cmd += " c.`address1` = '" + address + "' ";
                }
                if (!String.IsNullOrWhiteSpace(ssn))
                {
                    if (found)
                        cmd += " AND ";
                    found = true;
                    if (ssn.IndexOf("%") >= 0)
                        cmd += " p.`ssn` LIKE '" + ssn + "' ";
                    else
                        cmd += " p.`ssn` = '" + ssn + "' ";
                }

                if (!found)
                {
                    MessageBox.Show("***ERROR*** Nothing in which to search!");
                    return;
                }
                cmd += ";";

                dt = G1.get_db_data(cmd);
            }

            //            dt.Columns.Add("liability", Type.GetType("System.Double"));
            dt.Columns.Add("address");
            dt.Columns.Add("reports");

            //            dt.Columns.Add("policyNumber");
            //            dt.Columns.Add("policyFirstName");
            //            dt.Columns.Add("policyLastName");

            //DataTable testDt = filterSecNat(chkSecNat.Checked, dt);
            //dt = testDt.Copy();

            string contractNumber = "";
            double contractValue = 0D;
            string address1 = "";
            string city = "";
            string state = "";
            string zip = "";
            string payer = "";
            string policy = "";
            string record2 = "";
            string report = "";
            string reports = "";
            DateTime deceasedDate = DateTime.Now;
            DataRow[] dRows = null;
            DateTime birthDate = DateTime.Now;
            string companyCode = "";
            int row = 0;
            DataTable ddx = dt.Clone();
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                //contractValue = DailyHistory.GetContractValue(dt.Rows[i]);
                //contractValue = G1.RoundValue(contractValue);
                dt.Rows[i]["liability"] = contractValue;
                address1 = dt.Rows[i]["address1"].ObjToString() + " ";
                city = dt.Rows[i]["city"].ObjToString() + " ";
                state = dt.Rows[i]["state"].ObjToString() + "  ";
                zip = dt.Rows[i]["zip1"].ObjToString();
                if (!String.IsNullOrWhiteSpace(city))
                    address1 += city;
                if (!String.IsNullOrWhiteSpace(state))
                    address1 += state;
                if (!String.IsNullOrWhiteSpace(zip))
                    address1 += zip;
                dt.Rows[i]["address"] = address1;
                payer = dt.Rows[i]["payer"].ObjToString();
                //ddx.ImportRow(dt.Rows[i]);
                //G1.HardCopyDtRow(dt, i, ddx, ddx.Rows.Count);

                cmd = "Select * from `policies` where `payer` = '" + payer + "';";
                dx = G1.get_db_data(cmd);

                //testDt = filterSecNat(chkSecNat.Checked, dx);
                //dx = testDt.Copy();
                //if ( dx.Rows.Count > 0 )
                //    ddx.ImportRow(dt.Rows[i]);

                for (int j = 0; j < dx.Rows.Count; j++)
                {
                    contractValue = dx.Rows[j]["liability"].ObjToDouble();
                    policy = dx.Rows[j]["policyNumber"].ObjToString();
                    firstName = dx.Rows[j]["policyFirstName"].ObjToString();
                    lastName = dx.Rows[j]["policyLastName"].ObjToString();
                    birthDate = dx.Rows[j]["birthDate"].ObjToDateTime();
                    companyCode = dx.Rows[j]["companyCode"].ObjToString();
                    deceasedDate = dx.Rows[j]["deceasedDate"].ObjToDateTime();
                    record2 = dx.Rows[j]["record"].ObjToString();
                    report = dx.Rows[j]["report"].ObjToString();
                    if (record2 == "99047")
                    {

                    }
                    dRows = ddx.Select("record2='" + record2 + "'");
                    if (dRows.Length <= 0)
                    {
                        ddx.ImportRow(dt.Rows[i]);
                        //G1.HardCopyDtRow(dt, i, ddx, ddx.Rows.Count);
                        row = ddx.Rows.Count - 1;
                        ddx.Rows[row]["policyNumber"] = policy;
                        ddx.Rows[row]["liability"] = contractValue;
                        ddx.Rows[row]["policyFirstName"] = firstName;
                        ddx.Rows[row]["policyLastName"] = lastName;
                        ddx.Rows[row]["birthDate"] = G1.DTtoMySQLDT(birthDate);
                        ddx.Rows[row]["record2"] = record2;
                        ddx.Rows[row]["companyCode"] = companyCode;
                        ddx.Rows[row]["deceasedDate1"] = G1.DTtoMySQLDT(deceasedDate);
                        ddx.Rows[row]["deceasedDate"] = G1.DTtoMySQLDT(deceasedDate);
                        ddx.Rows[row]["report"] = report;
                    }
                    else
                    {
                        if (deceasedDate.Year > 100)
                        {
                            dRows[0]["deceasedDate1"] = G1.DTtoMySQLDT(deceasedDate);
                            dRows[0]["deceasedDate"] = G1.DTtoMySQLDT(deceasedDate);
                        }
                    }
                }
            }

            ProcessReports(ddx);

            G1.NumberDataTable(ddx);
            dgv2.DataSource = ddx;
            dgv2.Visible = true;
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private bool SearchInsurance( bool secondary = false )
        {
            dgv.Visible = false;
            string contract = this.txtContract.Text;
            string lastName = this.txtLastName.Text;
            string firstName = this.txtFirstName.Text;
            string address = this.txtAddress.Text;
            string ssn = this.txtSSN.Text;

            DataTable dt = null;
            DataTable dx = null;
            string cmd = "";
            string str = "";

            string contractFile = "icontracts";
            string customerFile = "icustomers";
            string policies = "policies";

            if (!String.IsNullOrWhiteSpace(contract))
            {
                cmd = "Select * from `" + customerFile + "` c JOIN `" + contractFile + "` m ON c.`contractNumber` = m.`contractNumber` where c.`contractNumber` = 'XYZZY';";
                dt = G1.get_db_data(cmd);

                cmd = "Select * from `" + customerFile + "` c JOIN `" + contractFile + "` m ON c.`contractNumber` = m.`contractNumber` where ";
                cmd += " c.`contractNumber` = '" + contract + "';";
                dt = G1.get_db_data(cmd);
                if (dt.Rows.Count <= 0)
                {
                    cmd = "Select * from `" + customerFile + "` c JOIN `" + contractFile + "` m ON c.`contractNumber` = m.`contractNumber` where ";
                    cmd += " `payer` = '" + contract + "' ";
                    //cmd += " AND c.`touched` IN ('A','L','D') ";
                    cmd += ";";
                    dx = G1.get_db_data(cmd);
                    if (dx.Rows.Count <= 0)
                    {
                        if (!secondary)
                        {
                            bool rv = SearchTrusts( false, true );
                            if (!rv)
                                MessageBox.Show("***ERROR*** Cannot locate any Insurance or Payer using " + contract + "!");
                            else
                                radioTrusts.Checked = true;
                        }
                        return false;
                    }
                    G1.HardCopyDataTable(dx, dt);
                }
            }
            else
            {

                bool found = false;
                this.Cursor = Cursors.WaitCursor;
                cmd = "Select * from `icustomers` c JOIN `icontracts` m ON c.`contractNumber` = m.`contractNumber` where c.`contractNumber` = 'XYZZY';";
                dt = G1.get_db_data(cmd);

                dgv.DataSource = dt;

                firstName = firstName.Replace("*", "%");
                lastName = lastName.Replace("*", "%");
                address = address.Replace("*", "%");
                ssn = ssn.Replace("*", "%");
                ssn = ssn.Replace("-", "");
                cmd = "Select * from `" + customerFile + "` c JOIN `" + contractFile + "` m ON c.`contractNumber` = m.`contractNumber` where ";
                if (!String.IsNullOrWhiteSpace(firstName))
                {
                    found = true;
                    if (firstName.IndexOf("%") >= 0)
                        cmd += " c.`firstName` LIKE '" + firstName + "' ";
                    else
                        cmd += " c.`firstName` = '" + firstName + "' ";
                }
                if (!String.IsNullOrWhiteSpace(lastName))
                {
                    if (found)
                        cmd += " AND ";
                    found = true;
                    if (lastName.IndexOf("%") >= 0)
                        cmd += " c.`lastName` LIKE '" + lastName + "' ";
                    else
                        cmd += " c.`lastName` = '" + lastName + "' ";
                }
                if (!String.IsNullOrWhiteSpace(address))
                {
                    if (found)
                        cmd += " AND ";
                    found = true;
                    if (address.IndexOf("%") >= 0)
                        cmd += " CONCAT_WS('', `address1`, `city`, `state`, `zip1` ) LIKE '" + address + "' ";
                    else
                        cmd += " c.`address1` = '" + address + "' ";
                }
                if (!String.IsNullOrWhiteSpace(ssn))
                {
                    if (found)
                        cmd += " AND ";
                    found = true;
                    if (ssn.IndexOf("%") >= 0)
                        cmd += " c.`ssn` LIKE '" + ssn + "' ";
                    else
                        cmd += " c.`ssn` = '" + ssn + "' ";
                }

                if (!found)
                {
                    if ( !secondary )
                        MessageBox.Show("***ERROR*** Nothing in which to search!");
                    return false;
                }
                cmd += ";";

                dt = G1.get_db_data(cmd);
            }

            dt.Columns.Add("liability", Type.GetType("System.Double"));
            dt.Columns.Add("address");
            dt.Columns.Add("policyNumber");
            dt.Columns.Add("policyFirstName");
            dt.Columns.Add("policyLastName");
            dt.Columns.Add("premium", Type.GetType("System.Double"));
            dt.Columns.Add("companyCode");
            dt.Columns.Add("report");
            dt.Columns.Add("reports");

            string contractNumber = "";
            double contractValue = 0D;
            string address1 = "";
            string city = "";
            string state = "";
            string zip = "";
            string payer = "";
            string policy = "";
            string lapsed = "";
            DateTime issueDate8 = DateTime.Now;
            DateTime deceasedDate = DateTime.Now;
            DateTime lapsedDate8 = DateTime.Now;
            DateTime emptyDate = new DateTime(1, 1, 1);
            DateTime dueDate8 = DateTime.Now;
            DateTime birthDate = DateTime.Now;
            double premium = 0D;
            string companyCode = "";
            string report = "";
            string reports = "";
            bool didit = false;
            int row = 0;
            DataRow [] dR = null;
            DataTable ddx = dt.Clone();
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                didit = false;
                contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                //contractValue = DailyHistory.GetContractValue(dt.Rows[i]);
                //contractValue = G1.RoundValue(contractValue);
                dt.Rows[i]["liability"] = 0D;
                address1 = dt.Rows[i]["address1"].ObjToString() + " ";
                city = dt.Rows[i]["city"].ObjToString() + " ";
                state = dt.Rows[i]["state"].ObjToString() + "  ";
                zip = dt.Rows[i]["zip1"].ObjToString();
                if (!String.IsNullOrWhiteSpace(city))
                    address1 += city;
                if (!String.IsNullOrWhiteSpace(state))
                    address1 += state;
                if (!String.IsNullOrWhiteSpace(zip))
                    address1 += zip;
                dt.Rows[i]["address"] = address1;
                payer = dt.Rows[i]["payer"].ObjToString();
                lapsed = dt.Rows[i]["lapsed1"].ObjToString();
                dt.Rows[i]["lapsed"] = lapsed;
                dt.Rows[i]["issueDate8"] = G1.DTtoMySQLDT(emptyDate);
                dueDate8 = dt.Rows[i]["dueDate8"].ObjToDateTime();
                if (i == 0)
                {
                    G1.HardCopyDtRow(dt, i, ddx, ddx.Rows.Count);
                    didit = true;
                }

                firstName = dt.Rows[i]["firstName"].ObjToString();
                lastName = dt.Rows[i]["lastName"].ObjToString();
                cmd = "Select * from `policies` p where `payer` = '" + payer + "' and `firstName` = '" + firstName + "' AND `lastName` = '" + lastName + "' AND `tmstamp` > '2020-01-01'";
                cmd += ";";

                dx = G1.get_db_data(cmd);

                //DataTable testDt = filterSecNat(chkSecNat.Checked, dx);
                //dx = testDt.Copy();

                if (dx.Rows.Count > 0)
                {
                    G1.HardCopyDtRow(dt, i, ddx, ddx.Rows.Count);
                }
                else
                {
                    if ( !didit )
                        G1.HardCopyDtRow(dt, i, ddx, ddx.Rows.Count);
                }

                for (int j = 0; j < dx.Rows.Count; j++)
                {
                    contractValue = dx.Rows[j]["liability"].ObjToDouble();
                    policy = dx.Rows[j]["policyNumber"].ObjToString();
                    firstName = dx.Rows[j]["policyFirstName"].ObjToString();
                    lastName = dx.Rows[j]["policyLastName"].ObjToString();
                    lapsed = dx.Rows[j]["lapsed"].ObjToString();
                    issueDate8 = dx.Rows[j]["issueDate8"].ObjToDateTime();
                    deceasedDate = dx.Rows[j]["deceasedDate"].ObjToDateTime();
                    lapsedDate8 = dx.Rows[j]["lapsedDate8"].ObjToDateTime();
                    premium = dx.Rows[j]["premium"].ObjToDouble();
                    birthDate = dx.Rows[j]["birthDate"].ObjToDateTime();
                    companyCode = dx.Rows[j]["companyCode"].ObjToString();
                    report = dx.Rows[j]["report"].ObjToString();
                    ddx.ImportRow(dt.Rows[i]);
                    //G1.HardCopyDtRow(dt, i, ddx, ddx.Rows.Count);
                    row = ddx.Rows.Count - 1;
                    ddx.Rows[row]["policyNumber"] = policy;
                    ddx.Rows[row]["liability"] = contractValue;
                    ddx.Rows[row]["policyFirstName"] = firstName;
                    ddx.Rows[row]["policyLastName"] = lastName;
                    ddx.Rows[row]["lapsed"] = lapsed;
                    ddx.Rows[row]["issueDate8"] = G1.DTtoMySQLDT(issueDate8);
                    ddx.Rows[row]["deceasedDate1"] = G1.DTtoMySQLDT(deceasedDate);
                    ddx.Rows[row]["lapseDate8"] = G1.DTtoMySQLDT(lapsedDate8);
                    ddx.Rows[row]["dueDate8"] = G1.DTtoMySQLDT(dueDate8);
                    ddx.Rows[row]["premium"] = premium;
                    ddx.Rows[row]["birthDate"] = G1.DTtoMySQLDT(birthDate);
                    ddx.Rows[row]["companyCode"] = companyCode;
                    ddx.Rows[row]["report"] = report;
                }
            }

            DataView tempview = ddx.DefaultView;
            tempview.Sort = "contractNumber DESC";
            ddx = tempview.ToTable();

            ProcessReports(ddx);

            FilterPolicies(ddx);

            //ddx = GroupPolicies(ddx);

            G1.NumberDataTable(ddx);

            dgv2.DataSource = ddx;
            dgv2.Visible = true;
            this.Cursor = Cursors.Default;
            return true;
        }
        /****************************************************************************************/
        private void ProcessReports ( DataTable dt )
        {
            //15068-1120 HTP
            //CG-WL11239 HSN
            //UC-1108 HNTP
            //JNA174-693 HSN + HNTP

            if (secNatDt == null)
                secNatDt = G1.get_db_data("Select * from `secnat`;");

            string companyCode = "";
            string report = "";
            string reports = "";
            string policy = "";
            DataRow[] dRows = null;
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                companyCode = dt.Rows[i]["companyCode"].ObjToString();
                report = dt.Rows[i]["report"].ObjToString();
                policy = dt.Rows[i]["policyNumber"].ObjToString();
                if (String.IsNullOrWhiteSpace(policy))
                    dt.Rows[i]["reports"] = "Payer";
                else
                {
                    dRows = secNatDt.Select("cc='" + companyCode + "'");
                    if (dRows.Length > 0)
                        dt.Rows[i]["reports"] = "SN";
                    else if (report.ToUpper() == "NOT THIRD PARTY")
                        dt.Rows[i]["reports"] = "NTP";
                    else if (String.IsNullOrWhiteSpace(report))
                        dt.Rows[i]["reports"] = "NTP";
                    else
                        dt.Rows[i]["reports"] = "TP";
                }
            }
        }
        /****************************************************************************************/
        public static void FilterPolicies ( DataTable dt )
        {
            string contractNumber = "";
            string cnum = "";
            string policy = "";
            DataRow[] dR = null;
            for ( int i=(dt.Rows.Count-1); i>=0; i--)
            {
                contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                policy = dt.Rows[i]["policyNumber"].ObjToString();
                if (String.IsNullOrWhiteSpace(policy))
                    continue;
                if ( contractNumber.IndexOf ("ZZ") >= 0 )
                {
                    dR = dt.Select("policyNumber='" + policy + "' AND contractNumber <> '" + contractNumber + "'");
                    if ( dR.Length > 0)
                    {
                        for ( int j=0; j<dR.Length; j++)
                        {
                            cnum = dR[j]["contractNumber"].ObjToString();
                            if (cnum.IndexOf("OO") >= 0 || cnum.IndexOf("MM") >= 0)
                                dR[j]["policyNumber"] = "XXX";
                        }
                    }
                }
            }
            DateTime oldDate = new DateTime(2019, 12, 31);
            for (int i = dt.Rows.Count - 1; i >= 0; i--)
            {
                policy = dt.Rows[i]["policyNumber"].ObjToString();
                if (policy == "XXX")
                    dt.Rows.RemoveAt(i);
                //else
                //{
                //    if (dt.Rows[i]["tmstamp"].ObjToDateTime() < oldDate)
                //        dt.Rows.RemoveAt(i);
                //}
            }
        }
        /****************************************************************************************/
        private DataTable GroupPolicies(DataTable dt)
        {
            if (dt.Rows.Count <= 0)
                return dt;

            DataTable groupDt = dt.AsEnumerable().GroupBy(r => new { Col1 = r["policyNumber"] }).Select(g => g.OrderBy(r => r["policyNumber"]).First()).CopyToDataTable();
            return groupDt;
        }
        /****************************************************************************************/
        private DataTable secNatDt = null;
        private DataTable filterSecNat ( bool include, DataTable dt)
        {
            if (secNatDt == null)
                secNatDt = G1.get_db_data("Select * from `secnat`;");

            if (!chkHonor.Checked)
                return dt;

            DataTable newDt = dt.Clone();
            try
            {
                if (!include)
                {
                    var result = dt.AsEnumerable()
                           .Where(row => !secNatDt.AsEnumerable()
                                                 .Select(r => r.Field<string>("cc"))
                                                 .Any(x => x == row.Field<string>("companyCode"))
                          ).CopyToDataTable();
                    newDt = result.Copy();
                }
                else
                {
                    var result = dt.AsEnumerable()
                           .Where(row => secNatDt.AsEnumerable()
                                                 .Select(r => r.Field<string>("cc"))
                                                 .Any(x => x == row.Field<string>("companyCode"))
                          ).CopyToDataTable();
                    newDt = result.Copy();
                }
            }
            catch ( Exception ex)
            {
            }
            return newDt;
        }
        /****************************************************************************************/
        private void SearchPayers()
        {
            dgv.Visible = false;
            dgv2.Visible = true;
            string contract = this.txtContract.Text;
            string lastName = this.txtLastName.Text;
            string firstName = this.txtFirstName.Text;
            string address = this.txtAddress.Text;
            string ssn = this.txtSSN.Text;

            DataTable dt = null;
            DataTable dx = null;
            string cmd = "";
            string str = "";

            string contractFile = "icontracts";
            string customerFile = "icustomers";
            string policies = "policies";

            //if (String.IsNullOrWhiteSpace(contract))
            //    return;

            DataTable ddt = null;
            if (!String.IsNullOrWhiteSpace(contract))
            {
                cmd = "Select * from `payers` where ";
                cmd += " `payer` = '" + contract + "';";
                ddt = G1.get_db_data(cmd);

                if (ddt.Rows.Count <= 0)
                {
                    dgv2.DataSource = null;
                    return;
                }
            }
            else
            {
                bool found = false;
                this.Cursor = Cursors.WaitCursor;

                firstName = firstName.Replace("*", "%");
                lastName = lastName.Replace("*", "%");
                address = address.Replace("*", "%");
                ssn = ssn.Replace("*", "%");
                ssn = ssn.Replace("-", "");
                cmd = "Select * from `payers` c JOIN `icontracts` m ON c.`contractNumber` = m.`contractNumber` JOIN `icustomers` n ON c.`contractNumber` = n.`contractNumber` where ";
                if (!String.IsNullOrWhiteSpace(firstName))
                {
                    found = true;
                    if (firstName.IndexOf("%") >= 0)
                        cmd += " c.`firstName` LIKE '" + firstName + "' ";
                    else
                        cmd += " c.`firstName` = '" + firstName + "' ";
                }
                if (!String.IsNullOrWhiteSpace(lastName))
                {
                    if (found)
                        cmd += " AND ";
                    found = true;
                    if (lastName.IndexOf("%") >= 0)
                        cmd += " c.`lastName` LIKE '" + lastName + "' ";
                    else
                        cmd += " c.`lastName` = '" + lastName + "' ";
                }
                if (!String.IsNullOrWhiteSpace(address))
                {
                    if (found)
                        cmd += " AND ";
                    found = true;
                    if (address.IndexOf("%") >= 0)
                        cmd += " CONCAT_WS('', `address1`, `city`, `state`, `zip1` ) LIKE '" + address + "' ";
                    else
                        cmd += " n.`address1` = '" + address + "' ";
                }
                if (!String.IsNullOrWhiteSpace(ssn))
                {
                    if (found)
                        cmd += " AND ";
                    found = true;
                    if (ssn.IndexOf("%") >= 0)
                        cmd += " n.`ssn` LIKE '" + ssn + "' ";
                    else
                        cmd += " n.`ssn` = '" + ssn + "' ";
                }

                if (!found)
                {
                    MessageBox.Show("***ERROR*** Nothing in which to search!");
                    return;
                }
                cmd += ";";

                ddt = G1.get_db_data(cmd);
                if ( ddt.Rows.Count <= 0)
                {
                    this.Cursor = Cursors.Default;
                    dgv2.DataSource = null;
                    return;
                }

            }

            DataTable ddx = null;
            for (int k = 0; k < ddt.Rows.Count; k++)
            {
                contract = ddt.Rows[k]["contractNumber"].ObjToString();

                cmd = "Select * from `" + customerFile + "` c JOIN `" + contractFile + "` m ON c.`contractNumber` = m.`contractNumber` where c.`contractNumber` = '" + contract + "';";
                dt = G1.get_db_data(cmd);

                dt.Columns.Add("liability", Type.GetType("System.Double"));
                dt.Columns.Add("address");
                dt.Columns.Add("policyNumber");
                dt.Columns.Add("policyFirstName");
                dt.Columns.Add("policyLastName");
                dt.Columns.Add("premium", Type.GetType("System.Double"));
                dt.Columns.Add("companyCode");
                dt.Columns.Add("report");
                dt.Columns.Add("reports");

                string contractNumber = "";
                double contractValue = 0D;
                string address1 = "";
                string city = "";
                string state = "";
                string zip = "";
                string payer = "";
                string policy = "";
                string lapsed = "";
                DateTime issueDate8 = DateTime.Now;
                DateTime deceasedDate = DateTime.Now;
                DateTime lapsedDate8 = DateTime.Now;
                DateTime emptyDate = new DateTime(1, 1, 1);
                DateTime dueDate8 = DateTime.Now;
                DateTime birthDate = DateTime.Now;
                double premium = 0D;
                string companyCode = "";
                string reports = "";
                string report = "";
                int row = 0;
                if ( ddx == null)
                   ddx = dt.Clone();
                if ( dt.Rows.Count == 0 )
                {
                    //G1.HardCopyDtRow(ddt, 0, ddx, ddx.Rows.Count);
                    contractNumber = ddt.Rows[0]["contractNumber"].ObjToString();
                    payer = ddt.Rows[0]["payer"].ObjToString();
                    lapsed = ddt.Rows[0]["lapsed"].ObjToString();
                    dueDate8 = ddt.Rows[0]["dueDate8"].ObjToDateTime();
                    //G1.HardCopyDtRow(dt, i, ddx, ddx.Rows.Count);

                    firstName = ddt.Rows[0]["firstName"].ObjToString();
                    lastName = ddt.Rows[0]["lastName"].ObjToString();

                    DataRow dr = ddx.NewRow();
                    dr["contractNumber"] = contractNumber;
                    dr["payer"] = payer;
                    dr["firstName"] = firstName;
                    dr["lastName"] = lastName;
                    dr["lapsed"] = lapsed;
                    dr["dueDate8"] = G1.DTtoMySQLDT(dueDate8);
                    dr["deceasedDate"] = G1.DTtoMySQLDT(ddt.Rows[0]["deceasedDate"].ObjToDateTime());
                    ddx.Rows.Add(dr);
                }

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                    //contractValue = DailyHistory.GetContractValue(dt.Rows[i]);
                    //contractValue = G1.RoundValue(contractValue);
                    dt.Rows[i]["liability"] = 0D;
                    address1 = dt.Rows[i]["address1"].ObjToString() + " ";
                    city = dt.Rows[i]["city"].ObjToString() + " ";
                    state = dt.Rows[i]["state"].ObjToString() + "  ";
                    zip = dt.Rows[i]["zip1"].ObjToString();
                    if (!String.IsNullOrWhiteSpace(city))
                        address1 += city;
                    if (!String.IsNullOrWhiteSpace(state))
                        address1 += state;
                    if (!String.IsNullOrWhiteSpace(zip))
                        address1 += zip;
                    dt.Rows[i]["address"] = address1;
                    payer = dt.Rows[i]["payer"].ObjToString();
                    lapsed = dt.Rows[i]["lapsed1"].ObjToString();
                    dt.Rows[i]["lapsed"] = lapsed;
                    dt.Rows[i]["issueDate8"] = G1.DTtoMySQLDT(emptyDate);
                    dueDate8 = dt.Rows[i]["dueDate8"].ObjToDateTime();
                    //G1.HardCopyDtRow(dt, i, ddx, ddx.Rows.Count);

                    firstName = dt.Rows[i]["firstName"].ObjToString();
                    lastName = dt.Rows[i]["lastName"].ObjToString();

                    cmd = "Select * from `policies` where `payer` = '" + payer + "' ;";
                    dx = G1.get_db_data(cmd);

                    //DataTable testDt = filterSecNat(chkSecNat.Checked, dx);
                    //dx = testDt.Copy();
                    //if ( dx.Rows.Count > 0 )
                        G1.HardCopyDtRow(dt, i, ddx, ddx.Rows.Count);

                    for (int j = 0; j < dx.Rows.Count; j++)
                    {
                        contractValue = dx.Rows[j]["liability"].ObjToDouble();
                        policy = dx.Rows[j]["policyNumber"].ObjToString();
                        firstName = dx.Rows[j]["policyFirstName"].ObjToString();
                        lastName = dx.Rows[j]["policyLastName"].ObjToString();
                        lapsed = dx.Rows[j]["lapsed"].ObjToString();
                        issueDate8 = dx.Rows[j]["issueDate8"].ObjToDateTime();
                        deceasedDate = dx.Rows[j]["deceasedDate"].ObjToDateTime();
                        lapsedDate8 = dx.Rows[j]["lapsedDate8"].ObjToDateTime();
                        premium = dx.Rows[j]["premium"].ObjToDouble();
                        birthDate = dx.Rows[j]["birthDate"].ObjToDateTime();
                        companyCode = dx.Rows[j]["companyCode"].ObjToString();
                        report = dx.Rows[j]["report"].ObjToString();
                        G1.HardCopyDtRow(dt, i, ddx, ddx.Rows.Count);
                        row = ddx.Rows.Count - 1;
                        ddx.Rows[row]["policyNumber"] = policy;
                        ddx.Rows[row]["liability"] = contractValue;
                        ddx.Rows[row]["policyFirstName"] = firstName;
                        ddx.Rows[row]["policyLastName"] = lastName;
                        ddx.Rows[row]["lapsed"] = lapsed;
                        ddx.Rows[row]["issueDate8"] = G1.DTtoMySQLDT(issueDate8);
                        ddx.Rows[row]["deceasedDate1"] = G1.DTtoMySQLDT(deceasedDate);
                        ddx.Rows[row]["lapseDate8"] = G1.DTtoMySQLDT(lapsedDate8);
                        ddx.Rows[row]["dueDate8"] = G1.DTtoMySQLDT(dueDate8);
                        ddx.Rows[row]["premium"] = premium;
                        ddx.Rows[row]["birthDate"] = G1.DTtoMySQLDT(birthDate);
                        ddx.Rows[row]["companyCode"] = companyCode;
                        ddx.Rows[row]["report"] = report;
                    }
                }
            }

            DataView tempview = ddx.DefaultView;
            tempview.Sort = "contractNumber DESC";
            ddx = tempview.ToTable();

            ProcessReports(ddx);

            FilterPolicies(ddx);

            G1.NumberDataTable(ddx);
            dgv2.DataSource = ddx;
            dgv2.Visible = true;
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private bool SearchTrusts( bool funerals = false, bool secondary = false )
        {
            dgv2.Visible = false;
            string contract = this.txtContract.Text;
            string lastName = this.txtLastName.Text;
            string firstName = this.txtFirstName.Text;
            string address = this.txtAddress.Text;
            string ssn = this.txtSSN.Text;

            DataTable dt = null;
            DataTable dx = null;
            string cmd = "";

            string contractFile = "contracts";
            string customerFile = "customers";
            if ( funerals )
            {
                contractFile = "fcontracts";
                customerFile = "fcustomers";
            }

            bool createdCustomer = false;

            if (!String.IsNullOrWhiteSpace(contract))
            {
                cmd = "Select * from `" + customerFile + "` c JOIN `" + contractFile + "` m ON c.`contractNumber` = m.`contractNumber` where c.`contractNumber` = 'XYZZY';";
                dt = G1.get_db_data(cmd);

                cmd = "Select * from `" + customerFile + "` c JOIN `" + contractFile + "` m ON c.`contractNumber` = m.`contractNumber` where ";
                cmd += " c.`contractNumber` = '" + contract + "';";
                dt = G1.get_db_data(cmd);
                if (dt.Rows.Count <= 0)
                {
                    if (secondary)
                        return false;
                    bool rv = SearchInsurance( true );
                    if (!rv)
                    {
                        cmd = "Select * from `" + contractFile + "` c JOIN `" + contractFile + "` m ON c.`contractNumber` = m.`contractNumber` where c.`contractNumber` = '" + contract + "';";
                        dx = G1.get_db_data(cmd);
                        if (dx.Rows.Count > 0)
                        {
                            DialogResult result = MessageBox.Show("***ERROR*** Customer Does Not Exist!\nDo you want to create it\nand then edit ?", "Customer Error Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                            if (result == DialogResult.No)
                            {
                                this.Cursor = Cursors.Default;
                                return false;
                            }
                            string rec = G1.create_record(customerFile, "zip1", "-1");
                            if ( !G1.BadRecord ( customerFile, rec ))
                            {
                                G1.update_db_table(customerFile, "record", rec, new string[] { "contractNumber", contract, "zip1", "" });
                                cmd = "Select * from `" + customerFile + "` c JOIN `" + contractFile + "` m ON c.`contractNumber` = m.`contractNumber` where ";
                                cmd += " c.`contractNumber` = '" + contract + "';";
                                dt = G1.get_db_data(cmd);
                                if ( dt.Rows.Count > 0 )
                                    createdCustomer = true;
                            }
                        }
                        else
                        {
                            MessageBox.Show("***ERROR*** Cannot locate any Contract using " + contract + "!", "Bad Contract Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                        }
                    }
                    else
                        this.radioInsurance.Checked = true;
                    if ( !createdCustomer )
                        return false;
                }
            }
            else
            {

                bool found = false;
                this.Cursor = Cursors.WaitCursor;
                cmd = "Select * from `customers` c JOIN `contracts` m ON c.`contractNumber` = m.`contractNumber` where c.`contractNumber` = 'XYZZY';";
                dt = G1.get_db_data(cmd);

                dgv.DataSource = dt;

                firstName = firstName.Replace("*", "%");
                lastName = lastName.Replace("*", "%");
                address = address.Replace("*", "%");
                ssn = ssn.Replace("*", "%");
                ssn = ssn.Replace("-", "");
                cmd = "Select * from `" + customerFile + "` c LEFT JOIN `" + contractFile + "` m ON c.`contractNumber` = m.`contractNumber` where ";
                if (!String.IsNullOrWhiteSpace(firstName))
                {
                    found = true;
                    if (firstName.IndexOf("%") >= 0)
                        cmd += " c.`firstName` LIKE '" + firstName + "' ";
                    else
                        cmd += " c.`firstName` = '" + firstName + "' ";
                }
                if (!String.IsNullOrWhiteSpace(lastName))
                {
                    if (found)
                        cmd += " AND ";
                    found = true;
                    if (lastName.IndexOf("%") >= 0)
                        cmd += " c.`lastName` LIKE '" + lastName + "' ";
                    else
                        cmd += " c.`lastName` = '" + lastName + "' ";
                }
                if (!String.IsNullOrWhiteSpace(address))
                {
                    if (found)
                        cmd += " AND ";
                    found = true;
                    if (address.IndexOf("%") >= 0)
                        cmd += " CONCAT_WS('', `address1`, `city`, `state`, `zip1` ) LIKE '" + address + "' ";
                    else
                        cmd += " c.`address1` = '" + address + "' ";
                }
                if (!String.IsNullOrWhiteSpace(ssn))
                {
                    if (found)
                        cmd += " AND ";
                    found = true;
                    if (ssn.IndexOf("%") >= 0)
                        cmd += " c.`ssn` LIKE '" + ssn + "' ";
                    else
                        cmd += " c.`ssn` = '" + ssn + "' ";
                }
                if (!found)
                {
                    MessageBox.Show("***ERROR*** Nothing in which to search!");
                    return false;
                }
                cmd += ";";

                dt = G1.get_db_data(cmd);
                if (dt.Rows.Count <= 0)
                {
                    cmd = cmd.Replace("LEFT", "RIGHT");
                    dt = G1.get_db_data(cmd);
                }
            }

            dt.Columns.Add("contractValue", Type.GetType("System.Double"));
            dt.Columns.Add("address");

            string contractNumber = "";
            double contractValue = 0D;
            string address1 = "";
            string city = "";
            string state = "";
            string zip = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                contractValue = DailyHistory.GetContractValue(dt.Rows[i]);
                contractValue = G1.RoundValue(contractValue);
                dt.Rows[i]["contractValue"] = contractValue;
                address1 = dt.Rows[i]["address1"].ObjToString() + " ";
                city = dt.Rows[i]["city"].ObjToString() + " ";
                state = dt.Rows[i]["state"].ObjToString() + "  ";
                zip = dt.Rows[i]["zip1"].ObjToString();
                if (!String.IsNullOrWhiteSpace(city))
                    address1 += city;
                if (!String.IsNullOrWhiteSpace(state))
                    address1 += state;
                if (!String.IsNullOrWhiteSpace(zip))
                    address1 += zip;
                dt.Rows[i]["address"] = address1;
            }
            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
            dgv.Visible = true;
            //if ( !String.IsNullOrWhiteSpace(ssn) && workContract.ToUpper().IndexOf("SX") == 0)
            if (!String.IsNullOrWhiteSpace(ssn))
            {
                if (dt.Rows.Count > 0 && !fromFuneral )
                {

                    btnPreneed.Show();
                    btnPreneed.Refresh();
                }
            }
            this.Cursor = Cursors.Default;
            return true;
        }
        /****************************************************************************************/
        private void gridMain_DoubleClick(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string contract = dr["contractNumber"].ObjToString();
            if (!String.IsNullOrWhiteSpace(contract))
            {
                this.Cursor = Cursors.WaitCursor;
                DataTable dt = (DataTable)dgv.DataSource;
                G1.UpdatePreviousCustomer(contract, LoginForm.username);
                bool insurance = false;
                if (contract.ToUpper().IndexOf("ZZ") == 0)
                    insurance = true;
                if (contract.ToUpper().IndexOf("MM") == 0)
                    insurance = true;
                if (contract.ToUpper().IndexOf("OO") == 0)
                    insurance = true;
                G1.UpdatePreviousCustomer(contract, LoginForm.username);
                if (insurance)
                {
                    string cmd = "Select * from `policies` p JOIN `icustomers` d ON p.`contractNumber` = d.`contractNumber` JOIN `icontracts` x ON p.`contractNumber` = x.`contractNumber` ";
                    cmd += " WHERE p.`contractNumber` = '" + contract + "' ";

                    DataTable dx = G1.get_db_data(cmd);
                    if (dx.Rows.Count > 0)
                    {
                        Policies policyForm = new Policies(contract);
                        policyForm.Show();
                    }
                    else
                    {
                        CustomerDetails clientForm = new CustomerDetails(contract);
                        clientForm.Show();
                    }
                }
                else
                {
                    if (radioFunerals.Checked)
                    {
                        EditCust custForm = new EditCust(contract);
                        custForm.Show();
                    }
                    else
                    {
                        CustomerDetails clientForm = new CustomerDetails(contract);
                        clientForm.Show();
                    }
                }
                this.Cursor = Cursors.Default;
            }
        }
        /****************************************************************************************/
        private void gridMain_CustomDrawCell(object sender, RowCellCustomDrawEventArgs e)
        {
            if (e.Column.FieldName.ToUpper().IndexOf("DATE") >= 0)
            {
                if (e.RowHandle >= 0)
                {
                    DateTime date = e.DisplayText.ObjToDateTime();
                    e.DisplayText = date.Month.ToString("D2") + "/" + date.Day.ToString("D2") + "/" + date.Year.ToString("D4");
                    if (date.Year <= 1)
                        e.DisplayText = "";
                }
            }
        }
        /****************************************************************************************/
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            if (dgv.Visible)
                SetSpyGlass(gridMain);
            else
                SetSpyGlass(gridMain2);
        }
        /***********************************************************************************************/
        private void SetSpyGlass(DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView grid)
        {
            if (grid.OptionsFind.AlwaysVisible == true)
                grid.OptionsFind.AlwaysVisible = false;
            else
                grid.OptionsFind.AlwaysVisible = true;
        }
        /****************************************************************************************/
        private void button1_Click(object sender, EventArgs e)
        {
            SearchFix searchForm = new SearchFix();
            searchForm.Show();
        }
        /***********************************************************************************************/
        private int pageMarginLeft = 0;
        private int pageMarginRight = 0;
        private int pageMarginTop = 0;
        private int pageMarginBottom = 0;
        /****************************************************************************************/
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
            printableComponentLink1.CreateDetailHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateDetailHeaderArea);
            printableComponentLink1.CreateMarginalHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateMarginalHeaderArea);
            printableComponentLink1.BeforeCreateAreas += new System.EventHandler(this.printableComponentLink1_BeforeCreateAreas);
            printableComponentLink1.AfterCreateAreas += new System.EventHandler(this.printableComponentLink1_AfterCreateAreas);
            printableComponentLink1.Landscape = true;

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
        /****************************************************************************************/
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
            printableComponentLink1.Landscape = true;

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
            //            footerCount = 0;
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
            string report = "General Search Program";
            Printer.DrawQuad(5, 8, 8, 4, report, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            //font = new Font("Ariel", 8, FontStyle.Regular);
            //report = cmbWhat.Text + " / " + cmbWho.Text + " / " + cmbDeposits.Text;
            //Printer.DrawQuad(10, 8, 2, 4, report, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            Printer.DrawQuadBorder(1, 1, 12, 12, BorderSide.All, 1, Color.Black);
            //            Printer.DrawQuadTicks();
        }
        /***************************************************************************************/
        public delegate void d_void_eventdone_string(string s);
        public event d_void_eventdone_string ListDone;
        protected void OnListDone(string s)
        {
            if (ListDone != null)
            {
                if (!string.IsNullOrWhiteSpace(s))
                {
                    ListDone.Invoke(s);
                    this.Close();
                }
            }
        }
        /****************************************************************************************/
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        /****************************************************************************************/
        private void btnSelect_Click(object sender, EventArgs e)
        {
            if (dgv.Visible)
            {
                if (dgv.DataSource == null)
                {
                    this.Close();
                    return;
                }
            }
            else if (dgv2.Visible)
            {
                if (dgv2.DataSource == null)
                {
                    this.Close();
                    return;
                }
            }
            DataTable dt = null;
            int row = -1;
            if (dgv.Visible)
            {
                dt = (DataTable)dgv.DataSource;
                row = gridMain.FocusedRowHandle;
                if (row < 0)
                    return;
                row = gridMain.GetDataSourceRowIndex(row);
            }
            else
            {
                try
                {
                    dt = (DataTable)dgv2.DataSource;
                    row = gridMain2.FocusedRowHandle;
                    if (row < 0)
                        return;
                    row = gridMain2.GetDataSourceRowIndex(row);
                    string record2 = dt.Rows[row]["record1"].ObjToString();
                }
                catch ( Exception ex)
                {
                    return;
                }
            }
            if ( dt == null)
            {
                MessageBox.Show("***ERROR*** You must first Search for a customer, then press Select Customer");
                return;
            }
            DataRow dr = null;
            if (radioInsurance.Checked || radioPolicies.Checked || radioPayers.Checked)
            {
                dr = gridMain2.GetFocusedDataRow();
                dr = dt.Rows[row];
            }
            else
            {
                dr = gridMain.GetFocusedDataRow();
                dr = dt.Rows[row];
            }
            string contract = dr["contractNumber"].ObjToString();
            string record = dr["record"].ObjToString();
            string report = "";
            if ( G1.get_column_number ( dt, "report") >= 0 )
                report = dr["report"].ObjToString();
            if (G1.get_column_number(dt, "record2") >= 0 )
                record = dr["record2"].ObjToString();
            else if ( G1.get_column_number ( dt, "record1") >= 0 )
                record = dr["record1"].ObjToString();
            if (!String.IsNullOrWhiteSpace(contract))
            {
                string payer = dr["payer"].ObjToString();
                string policy = "";
                string name = "";
                if (radioInsurance.Checked)
                {
                    string liability = dr["liability"].ObjToString();
                    string lastName = dr["policyLastName"].ObjToString();
                    if (String.IsNullOrWhiteSpace(lastName))
                        name = dr["lastName"].ObjToString() + ", " + dr["firstName"].ObjToString();
                    else
                        name = dr["policyLastName"].ObjToString() + ", " + dr["policyFirstName"].ObjToString();
                    payer = "Insurance : " + dr["policyNumber"].ObjToString() + " : Liability: " + liability + ":" + name + " : Payer:" + payer;
                    payer += " : PolicyRecord=" + record;
                    payer += " : Contract=" + contract;
                    contract = payer;
                }
                else if (radioPolicies.Checked)
                {
                    string liability = dr["liability"].ObjToString();
                    name = dr["policyLastName"].ObjToString() + ", " + dr["policyFirstName"].ObjToString();
                    policy = "Policy : " + dr["policyNumber"].ObjToString() + " : Liability: " + liability + " : " + name + " : Payer:" + payer;
                    policy += " : PolicyRecord=" + record;
                    policy += " : Contract=" + contract;
                    contract = policy;
                }
                else if (radioPayers.Checked)
                {
                    string liability = dr["liability"].ObjToString();
                    string lastName = dr["policyLastName"].ObjToString();
                    if ( String.IsNullOrWhiteSpace ( lastName))
                        name = dr["lastName"].ObjToString() + ", " + dr["firstName"].ObjToString();
                    else
                        name = dr["policyLastName"].ObjToString() + ", " + dr["policyFirstName"].ObjToString();
                    policy = "Policy : " + dr["policyNumber"].ObjToString() + " : Liability: " + liability + " : " + name + " : Payer:" + payer;
                    policy += " : PolicyRecord=" + record;
                    policy += " : Contract=" + contract;
                    contract = policy;
                }
                else if (radioTrusts.Checked)
                {
                    string contractValue = dr["contractValue"].ObjToString();
                    name = dr["lastName"].ObjToString() + ", " + dr["firstName"].ObjToString();
                    policy = "Trust : " + dr["contractNumber"].ObjToString() + " : ContractValue : " + contractValue + " : " + name;
                    policy += " : Contract=" + contract;
                    contract = policy;
                }
                OnListDone(contract);
            }
        }
        /****************************************************************************************/
        private void radioTrusts_CheckedChanged(object sender, EventArgs e)
        {
            dgv2.Visible = false;
            dgv.Visible = true;
            chkFilter.Hide();
            chkPayersOnly.Hide();
        }
        /****************************************************************************************/
        private void radioInsurance_CheckedChanged(object sender, EventArgs e)
        {
            dgv2.Visible = true;
            dgv.Visible = false;
            chkFilter.Show();
            string preference = G1.getPreference(LoginForm.username, "Allow Filter Deceased in Lookups", "Allow Access");
            if (preference != "YES")
                chkFilter.Hide();
            chkPayersOnly.Show();
        }
        /****************************************************************************************/
        private void radioPolicies_CheckedChanged(object sender, EventArgs e)
        {
            dgv2.Visible = true;
            dgv.Visible = false;
            chkFilter.Show();
            string preference = G1.getPreference(LoginForm.username, "Allow Filter Deceased in Lookups", "Allow Access");
            if (preference != "YES")
                chkFilter.Hide();
            chkPayersOnly.Show();
        }
        /****************************************************************************************/
        private void radioPayers_CheckedChanged(object sender, EventArgs e)
        {
            dgv2.Visible = true;
            dgv.Visible = false;
            chkFilter.Show();
            string preference = G1.getPreference(LoginForm.username, "Allow Filter Deceased in Lookups", "Allow Access");
            if (preference != "YES")
                chkFilter.Hide();

            chkPayersOnly.Show();
        }
        /****************************************************************************************/
        private void gridMain2_DoubleClick(object sender, EventArgs e)
        {
            DataRow dr = gridMain2.GetFocusedDataRow();
            string contract = dr["contractNumber"].ObjToString();
            string record2 = "";
            bool gotRecord2 = false;
            try
            {
                record2 = dr["record2"].ObjToString();
                gotRecord2 = true;
            }
            catch ( Exception ex)
            {
            }
            if (!String.IsNullOrWhiteSpace(contract))
            {
                if ( radioPayers.Checked )
                {
                    this.Cursor = Cursors.WaitCursor;
                    CustomerDetails clientForm = new CustomerDetails(contract);
                    clientForm.Show();
                    this.Cursor = Cursors.Default;
                    return;
                }
                this.Cursor = Cursors.WaitCursor;
                DataTable dt = (DataTable)dgv2.DataSource;
                G1.UpdatePreviousCustomer(contract, LoginForm.username);
                string cmd = "Select * from `policies` p JOIN `icustomers` d ON p.`contractNumber` = d.`contractNumber` JOIN `icontracts` x ON p.`contractNumber` = x.`contractNumber` ";
                cmd += " WHERE p.`contractNumber` = '" + contract + "' ";

                DataTable dx = G1.get_db_data(cmd);
                if (dx.Rows.Count > 0 && 1 != 1 )
                {
                    Policies policyForm = new Policies(contract);
                    policyForm.Show();
                }
                else
                {
                    CustomerDetails clientForm = new CustomerDetails(contract);
                    clientForm.Show();
                }
                this.Cursor = Cursors.Default;
            }
        }
        /****************************************************************************************/
        private void txtContract_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Tab)
            {
                string search = txtContract.Text.Trim();
                if (!String.IsNullOrWhiteSpace(search))
                    btnRun_Click(null, null);
            }
        }
        /****************************************************************************************/
        private void gridMain2_CustomColumnDisplayText(object sender, CustomColumnDisplayTextEventArgs e)
        {
            ColumnView view = sender as ColumnView;
            if (e.Column.FieldName.ToUpper().IndexOf("DATE") >= 0 && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
            {
                if (e.DisplayText.IndexOf("0000") >= 0 || e.DisplayText.IndexOf("0001") >= 0)
                    e.DisplayText = "";
                else
                {
                    DateTime date = e.DisplayText.ObjToDateTime();
                    if ( date.Year < 100)
                        e.DisplayText = "";
                    else
                        e.DisplayText = date.ToString("MM/dd/yyyy");
                }
            }
        }
        /****************************************************************************************/
        private void gridMain2_CustomRowFilter(object sender, RowFilterEventArgs e)
        {
            string policy = "";
            if (!chkFilter.Checked )
                return;
            int row = e.ListSourceRow;
            DataTable dt = (DataTable)dgv2.DataSource;
            if (dt == null)
                return;
            if (!chkFilter.Checked)
            {
                if (!chkPayersOnly.Checked)
                {
                    FilterPolicies(sender, ref e);
                    return;
                }
            }
            ColumnView view = sender as ColumnView;
            if ( chkPayersOnly.Checked)
            {
                policy = dt.Rows[row]["policyNumber"].ObjToString();
                if ( !String.IsNullOrWhiteSpace ( policy))
                {
                    e.Visible = false;
                    e.Handled = true;
                    return;
                }
            }
            string contractNumber = dt.Rows[row]["contractNumber"].ObjToString();
            if ( contractNumber.IndexOf ( "OO") >= 0 )
            {
                e.Visible = false;
                e.Handled = true;
                return;
            }
            if (contractNumber.IndexOf("MM") >= 0)
            {
                e.Visible = false;
                e.Handled = true;
                return;
            }
            DateTime deceasedDate = dt.Rows[row]["deceasedDate"].ObjToDateTime();
            if ( deceasedDate.Year > 100)
            {
                e.Visible = false;
                e.Handled = true;
                return;
            }
            deceasedDate = dt.Rows[row]["deceasedDate1"].ObjToDateTime();
            if (deceasedDate.Year > 100)
            {
                e.Visible = false;
                e.Handled = true;
                return;
            }
            DateTime lapsedDate = dt.Rows[row]["lapseDate8"].ObjToDateTime();
            if (lapsedDate.Year > 100)
            {
                e.Visible = false;
                e.Handled = true;
                return;
            }
            string lapsed = dt.Rows[row]["lapsed"].ObjToString();
            if (lapsed == "Y")
            {
                e.Visible = false;
                e.Handled = true;
                return;
            }
        }
        /****************************************************************************************/
        private void FilterPolicies (object sender, ref RowFilterEventArgs e)
        {
            if (radioPolicies.Checked)
                return;
            int row = e.ListSourceRow;
            DataTable dt = (DataTable)dgv2.DataSource;
            if (dt == null)
                return;

            DateTime date = DateTime.Now;
            string report = dt.Rows[row]["report"].ObjToString();
            string reports = dt.Rows[row]["reports"].ObjToString();
            string policy = dt.Rows[row]["policyNumber"].ObjToString();
            if (String.IsNullOrWhiteSpace(policy))
                return;
            if (chkHonor.Checked)
            {

                if (date > DailyHistory.killSecNatDate)
                {
                    if (reports == "SN")
                    {
                        e.Visible = false;
                        e.Handled = true;
                        return;
                    }
                }
                if (date > DailyHistory.kill3rdPartyDate)
                {
                    if (reports == "TP")
                    {
                        e.Visible = false;
                        e.Handled = true;
                        return;
                    }
                }
                return;
            }
            if (chkThirdOnly.Checked && chkSecNat.Checked)
            {
                if (reports == "TP" || reports == "SN")
                    return;
                e.Visible = false;
                e.Handled = true;
                return;
            }
            if (chkThirdOnly.Checked)
            {
                if (reports == "TP")
                    return;
                e.Visible = false;
                e.Handled = true;
                return;
            }
            if (chkSecNat.Checked)
            {
                if (reports == "SN")
                    return;
                e.Visible = false;
                e.Handled = true;
                return;
            }
        }
        /****************************************************************************************/
        private void chkFilter_CheckedChanged(object sender, EventArgs e)
        {
            bool goit = chkFilter.Checked;
            gridMain2.RefreshData();
            dgv2.Refresh();
        }
        /****************************************************************************************/
        private void chkPayersOnly_CheckedChanged(object sender, EventArgs e)
        {
            gridMain2.RefreshData();
            dgv2.Refresh();
        }
        /****************************************************************************************/
        private void makeThisTheActivePayerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv2.DataSource;
            DataRow dr = gridMain2.GetFocusedDataRow();
            string record = dr["record"].ObjToString();
            string contractNumber = dr["contractNumber"].ObjToString();
            string payer = dr["payer"].ObjToString();
            DialogResult result = MessageBox.Show("Are you sure you want to make this (" + contractNumber + ") the active Payer Contract?", "Change Active Payer Contract Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
            if (result == DialogResult.No)
                return;
            string cmd = "Select * from `payers` where `payer` = '" + payer + "';";
            DataTable dx = G1.get_db_data(cmd);
            if ( dx.Rows.Count > 0 )
            {
                string payerRecord = dx.Rows[0]["record"].ObjToString();
                DateTime dueDate8 = dr["dueDate8"].ObjToDateTime();
                double premium = Policies.CalcMonthlyPremium(payer, dueDate8);
                double annual = dr["annualPremium"].ObjToDouble();
                DateTime dolp = dr["lastDatePaid8"].ObjToDateTime();
                DateTime lapseDate = dr["lapseDate8"].ObjToDateTime();
                DateTime reinstateDate = dr["reinstateDate8"].ObjToDateTime();
                DateTime deceasedDate = dr["deceasedDate"].ObjToDateTime();
                string lapsed = dr["lapsed"].ObjToString();
                string lastName = dr["lastName"].ObjToString();
                string firstName = dr["firstName"].ObjToString();

                string dueDateNew = dueDate8.ToString("yyyy-MM-dd");
                string dolpNew = dolp.ToString("yyyy-MM-dd");
                string lapseDateNew = lapseDate.ToString("yyyy-MM-dd");
                string reinstateDateNew = reinstateDate.ToString("yyyy-MM-dd");
                string deceasedDateNew = deceasedDate.ToString("yyyy-MM-dd");

                G1.update_db_table("payers", "record", payerRecord, new string[] { "contractNumber", contractNumber, "dueDate8", dueDateNew, "amtOfMonthlyPayt", premium.ToString(), "annualPremium", annual.ToString()});
                G1.update_db_table("payers", "record", payerRecord, new string[] { "lastName", lastName, "firstName", firstName, "lastDatePaid8", dolpNew, "lapseDate8", lapseDateNew, "lapsed", lapsed, "reinstateDate8", reinstateDateNew, "deceasedDate", deceasedDateNew });
            }
            else
            {
                string payerRecord = G1.create_record("payers", "firstName", "-1");
                if (G1.BadRecord("payer", payerRecord))
                    return;

                DateTime dueDate8 = dr["dueDate8"].ObjToDateTime();
                double premium = Policies.CalcMonthlyPremium(payer, dueDate8);
                double annual = dr["annualPremium"].ObjToDouble();
                DateTime dolp = dr["lastDatePaid8"].ObjToDateTime();
                DateTime lapseDate = dr["lapseDate8"].ObjToDateTime();
                DateTime reinstateDate = dr["reinstateDate8"].ObjToDateTime();
                DateTime deceasedDate = dr["deceasedDate"].ObjToDateTime();
                string lapsed = dr["lapsed"].ObjToString();
                string lastName = dr["lastName"].ObjToString();
                string firstName = dr["firstName"].ObjToString();

                string dueDateNew = dueDate8.ToString("yyyy-MM-dd");
                string dolpNew = dolp.ToString("yyyy-MM-dd");
                string lapseDateNew = lapseDate.ToString("yyyy-MM-dd");
                string reinstateDateNew = reinstateDate.ToString("yyyy-MM-dd");
                string deceasedDateNew = deceasedDate.ToString("yyyy-MM-dd");

                G1.update_db_table("payers", "record", payerRecord, new string[] { "contractNumber", contractNumber, "dueDate8", dueDateNew, "amtOfMonthlyPayt", premium.ToString(), "annualPremium", annual.ToString(), "payer", payer });
                G1.update_db_table("payers", "record", payerRecord, new string[] { "lastName", lastName, "firstName", firstName, "lastDatePaid8", dolpNew, "lapseDate8", lapseDateNew, "lapsed", lapsed, "reinstateDate8", reinstateDateNew, "deceasedDate", deceasedDateNew });
            }
        }
        /****************************************************************************************/
        private void deleteEntireContractToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!G1.ValidateOverridePassword ( "Enter Password To Delete Contract > " ))
                return;

            //using (Ask fmrmyform = new Ask("Enter Password To Delete Contract > "))
            //{
            //    fmrmyform.Text = "";
            //    fmrmyform.ShowDialog();
            //    string answer = fmrmyform.Answer.Trim().ToUpper();
            //    if (String.IsNullOrWhiteSpace(answer))
            //        return; // Loser!
            //    if (answer.ToUpper() != "SOSO")
            //    {
            //        MessageBox.Show("***ERROR*** Invalid Password!!!");
            //        return;
            //    }
            //}
            DataTable dt = (DataTable)dgv.DataSource;
            DataRow dr = gridMain.GetFocusedDataRow();
            string record = dr["record"].ObjToString();
            string contractNumber = dr["contractNumber"].ObjToString();

            string cmd = "Select * from `customers` where `contractNumber` = '" + contractNumber + "';";
            DataTable dx = G1.get_db_data(cmd);
            if ( dx.Rows.Count > 0 )
            {
                record = dx.Rows[0]["record"].ObjToString();
                G1.delete_db_table("customers", "record", record);
            }

            cmd = "Select * from `contracts` where `contractNumber` = '" + contractNumber + "';";
            dx = G1.get_db_data(cmd);
            if (dx.Rows.Count > 0)
            {
                record = dx.Rows[0]["record"].ObjToString();
                G1.delete_db_table("contracts", "record", record);
            }

            cmd = "Select * from `payments` where `contractNumber` = '" + contractNumber + "';";
            dx = G1.get_db_data(cmd);
            if (dx.Rows.Count > 0)
            {
                for (int i = 0; i < dx.Rows.Count; i++)
                {
                    record = dx.Rows[i]["record"].ObjToString();
                    G1.delete_db_table("payments", "record", record);
                }
            }

            cmd = "Select * from `cust_services` where `contractNumber` = '" + contractNumber + "';";
            dx = G1.get_db_data(cmd);
            if (dx.Rows.Count > 0)
            {
                for (int i = 0; i < dx.Rows.Count; i++)
                {
                    record = dx.Rows[i]["record"].ObjToString();
                    G1.delete_db_table("cust_services", "record", record);
                }
            }

            MessageBox.Show("***INFO*** Contract (" + contractNumber + ") Deleted!", "Delete Contract Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);

        }
        /****************************************************************************************/
        private void chkHonor_CheckedChanged(object sender, EventArgs e)
        {
            if (dgv2 != null)
            {
                gridMain2.RefreshData();
                dgv2.Refresh();
            }
        }
        /****************************************************************************************/
        private void chkSecNat_CheckedChanged(object sender, EventArgs e)
        {
            if (dgv2 != null)
            {
                gridMain2.RefreshData();
                dgv2.Refresh();
            }
        }
        /****************************************************************************************/
        private void chkThirdOnly_CheckedChanged(object sender, EventArgs e)
        {
            if (dgv2 != null)
            {
                gridMain2.RefreshData();
                dgv2.Refresh();
            }
        }
        /****************************************************************************************/
        private void btnPreneed_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string contract = dr["contractNumber"].ObjToString();
            if (String.IsNullOrWhiteSpace(contract))
                return;
            string ssn = this.txtSSN.Text.Trim();
            if (String.IsNullOrWhiteSpace(ssn))
                return;
            if (checkThePreneed(contract, ssn))
            {
                this.Cursor = Cursors.WaitCursor;
                OnListDone("Get Out! " + contract );
                this.Cursor = Cursors.Default;
            }
        }
        /***********************************************************************************************/
        private bool checkThePreneed( string contractNumber, string ssn )
        {
            if (String.IsNullOrWhiteSpace(ssn))
                return false;
            if (ssn == "0")
                return false;
            if (ssn == "1")
                return false;

            DataTable dt = null;
            string cmd = "";

            if (workContract.ToUpper().IndexOf("SX") == 0)
            {
                cmd = "Select * from `fcustomers` where `contractNumber` = '" + contractNumber + "';";
                dt = G1.get_db_data(cmd);
                if (dt.Rows.Count > 0)
                {
                    DevExpress.XtraEditors.XtraMessageBox.Show("***ERROR***\nFuneral Already Exists for Contract (" + contractNumber + ")!", "Funeral Already Exists Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                    return false;
                }
            }

            //funContract = "";
            cmd = "Select * from `fcustomers` where `contractNumber` = '" + workContract + "';";
            dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                return false;

            string funContract = dt.Rows[0]["contractNumber"].ObjToString();
            DialogResult result = DevExpress.XtraEditors.XtraMessageBox.Show("***Question***\nPreNeed exists for SSN " + ssn + "\nUnder Contract (" + contractNumber + ")!\n" + "Would you like to convert Funeral Contract (" + workContract + ")\nto Preneed Contract (" + contractNumber + ")?", "PreNeed Exists Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
            if (result == DialogResult.No)
            {
                funContract = "";
                return false;
            }

            string serviceId = dt.Rows[0]["serviceId"].ObjToString();
            DateTime deceasedDate = dt.Rows[0]["deceasedDate"].ObjToDateTime();

            string record = dt.Rows[0]["record"].ObjToString();
            ssn = ssn.Replace("-", "");

            G1.update_db_table("fcustomers", "record", record, new string[] { "contractNumber", contractNumber, "ssn", ssn });

            cmd = "Select * from `fcontracts` where `contractNumber` = '" + funContract + "';";
            dt = G1.get_db_data(cmd);
            if (dt.Rows.Count > 0)
            {
                record = dt.Rows[0]["record"].ObjToString();
                G1.update_db_table("fcontracts", "record", record, new string[] { "contractNumber", contractNumber });
            }

            cmd = "Select * from `fcust_extended` where `contractNumber` = '" + funContract + "';";
            dt = G1.get_db_data(cmd);
            if (dt.Rows.Count > 0)
            {
                DateTime serviceDate = dt.Rows[0]["serviceDate"].ObjToDateTime();
                DateTime arrangementDate = dt.Rows[0]["arrangementDate"].ObjToDateTime();
                record = dt.Rows[0]["record"].ObjToString();
                G1.update_db_table("fcust_extended", "record", record, new string[] { "contractNumber", contractNumber });

                cmd = "Select * from `cust_extended` where `contractNumber` = '" + funContract + "';";
                dt = G1.get_db_data(cmd);
                if (dt.Rows.Count > 0)
                {
                    record = dt.Rows[0]["record"].ObjToString();
                    G1.delete_db_table("cust_extended", "record", record);
                }
                CustomerDetails.CopyFromTableToTable("fcust_extended", "cust_extended", contractNumber);
            }

            cmd = "Select * from `fcust_services` where `contractNumber` = '" + funContract + "';";
            dt = G1.get_db_data(cmd);
            if (dt.Rows.Count > 0)
            {
                for (int j = 0; j < dt.Rows.Count; j++)
                {
                    record = dt.Rows[j]["record"].ObjToString();
                    G1.update_db_table("fcust_services", "record", record, new string[] { "contractNumber", contractNumber });
                }
            }
            else
            {
                cmd = "Select * from `cust_services` where `contractNumber` = '" + contractNumber + "';";
                dt = G1.get_db_data(cmd);
                if ( dt.Rows.Count > 0 )
                {
                    CustomerDetails.CopyFromTableToTable("cust_services", "fcust_services", contractNumber );
                }
            }

            cmd = "Select * from `cust_payments` where `contractNumber` = '" + funContract + "';";
            dt = G1.get_db_data(cmd);
            if (dt.Rows.Count > 0)
            {
                for (int j = 0; j < dt.Rows.Count; j++)
                {
                    record = dt.Rows[j]["record"].ObjToString();
                    G1.update_db_table("cust_payments", "record", record, new string[] { "contractNumber", contractNumber });
                }
            }

            cmd = "Select * from `cust_payment_details` where `contractNumber` = '" + funContract + "';";
            dt = G1.get_db_data(cmd);
            if (dt.Rows.Count > 0)
            {
                for (int j = 0; j < dt.Rows.Count; j++)
                {
                    record = dt.Rows[j]["record"].ObjToString();
                    G1.update_db_table("cust_payment_details", "record", record, new string[] { "contractNumber", contractNumber });
                }
            }

            cmd = "Select * from `cust_payment_ins_checklist` where `contractNumber` = '" + funContract + "';";
            dt = G1.get_db_data(cmd);
            if (dt.Rows.Count > 0)
            {
                for (int j = 0; j < dt.Rows.Count; j++)
                {
                    record = dt.Rows[j]["record"].ObjToString();
                    G1.update_db_table("cust_payment_ins_checklist", "record", record, new string[] { "contractNumber", contractNumber });
                }
            }

            cmd = "Select * from `relatives` where `contractNumber` = '" + funContract + "';";
            dt = G1.get_db_data(cmd);
            if (dt.Rows.Count > 0)
            {
                for (int j = 0; j < dt.Rows.Count; j++)
                {
                    record = dt.Rows[j]["record"].ObjToString();
                    G1.update_db_table("relatives", "record", record, new string[] { "contractNumber", contractNumber });
                }
            }

            cmd = "Select * from `contracts` where `contractNumber` = '" + contractNumber + "';";
            dt = G1.get_db_data(cmd);
            if (dt.Rows.Count > 0)
            {
                record = dt.Rows[0]["record"].ObjToString();
                G1.update_db_table("contracts", "record", record, new string[] { "deceasedDate", deceasedDate.ToString("yyyy-MM-dd") });
            }

            cmd = "Select * from `customers` where `contractNumber` = '" + contractNumber + "';";
            dt = G1.get_db_data(cmd);
            if (dt.Rows.Count > 0)
            {
                record = dt.Rows[0]["record"].ObjToString();
                G1.update_db_table("customers", "record", record, new string[] { "ServiceId", serviceId, "deceasedDate", deceasedDate.ToString("yyyy-MM-dd") });
            }

            if (funContract == contractNumber && funContract.ToUpper().IndexOf("SX") != 0)
            {
                cmd = "DELETE from `fcust_services` where `contractNumber` = '" + contractNumber + "';";
                G1.get_db_data(cmd);

                CustomerDetails.CopyFromTableToTable("cust_services", "fcust_services", contractNumber);
                return true;
            }

            FunServices serviceForm = new FunServices(contractNumber, false, true);
            DataTable dx = serviceForm.funServicesDT;
            serviceForm.Dispose();
            serviceForm = null;

            serviceForm = new FunServices(contractNumber);
            dt = serviceForm.funServicesDT;
            serviceForm.Dispose();
            serviceForm = null;

            string service = "";
            string price = "";
            string currentPrice = "";
            string curPrice = "";
            DataRow[] dRows = null;

            string whatMerchandise = "";

            for (int i = 0; i < dx.Rows.Count; i++)
            {
                service = dx.Rows[i]["service"].ObjToString();
                price = dx.Rows[i]["price"].ObjToString();
                currentPrice = dx.Rows[i]["currentPrice"].ObjToString();
                dRows = dt.Select("service='" + service + "'");
                if (dRows.Length > 0)
                {
                    if (price.ObjToDouble() == 0D && currentPrice.ObjToDouble() > 0D)
                        price = currentPrice;
                    record = dRows[0]["record"].ObjToString();
                    G1.update_db_table("fcust_services", "record", record, new string[] { "price", price });
                }
                else
                {
                    whatMerchandise = FunServices.isWhatMerchandise(service);
                    if (whatMerchandise.ToUpper() == "CASKET")
                    {
                        if ( locateCasket ( dt, ref service, ref record , ref curPrice ) )
                        {
                            if ( curPrice == currentPrice )
                                G1.update_db_table("fcust_services", "record", record, new string[] { "price", price });
                        }
                    }
                }
            }
            return true;
        }
        /****************************************************************************************/
        private bool locateCasket ( DataTable dt, ref string service, ref string record, ref string currentPrice )
        {
            bool found = false;
            string data = "";
            string casket = "";
            string whatMerchandise = "";
            service = "";
            record = "";
            currentPrice = "";

            for ( int i=0; i<dt.Rows.Count; i++)
            {
                data = dt.Rows[i]["service"].ObjToString();
                whatMerchandise = FunServices.isWhatMerchandise(data);
                if ( whatMerchandise.ToUpper() == "CASKET")
                {
                    found = true;
                    service = data;
                    record = dt.Rows[i]["record"].ObjToString();
                    currentPrice = dt.Rows[i]["currentPrice"].ObjToString();
                    break;
                }
            }
            return found;
        }
        /****************************************************************************************/
        private void showDeathPayoffToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }
        /****************************************************************************************/
        private void showCustomerPayoffIn10DaysToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }
        /****************************************************************************************/
        private void deathPayoffAsOfTodayToolStripMenuItem_Click(object sender, EventArgs e)
        {
            customerPayoff("Packet Payoff");
        }
        /****************************************************************************************/
        private void customerPayoffAsOf10DaysToolStripMenuItem_Click(object sender, EventArgs e)
        {
            customerPayoff("Packet Payoff-10");
        }
        /****************************************************************************************/
        private void customerPayoff ( string whatPayoff )
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string contractNumber = dr["contractNumber"].ObjToString();

            string historyFile = @"C:/rag/pdfDaily.pdf";
            G1.GrantFileAccess(historyFile);
            //DailyHistory histForm = new DailyHistory(contractNumber, historyFile, true, "Packet Payoff");
            DailyHistory histForm = new DailyHistory(contractNumber, historyFile, true, whatPayoff );

            //string outputPdfPath = historyFile;

            string pdf1 = @"c:/rag/workPDF1.pdf";
            string pdf2 = @"c:/rag/workPDF2.pdf";
            string pdf3 = @"c:/rag/workPDF3.pdf";

            G1.GrantFileAccess(pdf1);
            G1.GrantFileAccess(pdf2);
            G1.GrantFileAccess(pdf3);

            iTextSharp.text.Document sourceDocument = null;
            PdfCopy pdfCopyProvider = null;
            PdfImportedPage importedPage;

            string outputPdfPath = @"C:/rag/pdfX.pdf";
            G1.GrantFileAccess(outputPdfPath);

            if (File.Exists(outputPdfPath))
            {
                File.SetAttributes(outputPdfPath, FileAttributes.Normal);
                File.Delete(outputPdfPath);
            }

            sourceDocument = new iTextSharp.text.Document();
            pdfCopyProvider = new PdfCopy(sourceDocument, new System.IO.FileStream(outputPdfPath, System.IO.FileMode.Create));

            //output file Open  
            sourceDocument.Open();

            MergeAllPDF(pdfCopyProvider, pdf1, historyFile, pdf2, pdf3);

            if (File.Exists(pdf1))
            {
                File.SetAttributes(pdf1, FileAttributes.Normal);
                File.Delete(pdf1);
            }
            if (File.Exists(historyFile))
            {
                File.SetAttributes(historyFile, FileAttributes.Normal);
                File.Delete(historyFile);
            }
            if (File.Exists(pdf2))
            {
                File.SetAttributes(pdf2, FileAttributes.Normal);
                File.Delete(pdf2);
            }
            if (File.Exists(pdf3))
            {
                File.SetAttributes(pdf3, FileAttributes.Normal);
                File.Delete(pdf3);
            }

            sourceDocument.Close();

            ViewPDF myView = new ViewPDF("TEST", outputPdfPath);
            myView.ShowDialog();

            if (File.Exists(outputPdfPath))
            {
                File.SetAttributes(outputPdfPath, FileAttributes.Normal);
                File.Delete(outputPdfPath);
            }
        }
        /***********************************************************************************************/
        //private bool GeneratePages = false;
        //private void btnGenerate_Click(object sender, EventArgs e)
        //{
        //    DataTable dt = (DataTable)dgv.DataSource;
        //    if (dt == null)
        //        return;
        //    if (dt.Rows.Count <= 0)
        //        return;

        //    int[] rows = gridMain.GetSelectedRows();

        //    int firstRow = 0;
        //    int lastRow = dt.Rows.Count;
        //    if (rows.Length <= 0)
        //        return;
        //    int row = 0;
        //    DataTable dx = dt.Clone();
        //    DataRow dRow = null;
        //    for (int i = 0; i < rows.Length; i++)
        //    {
        //        row = rows[i];
        //        if (row < 0)
        //            continue;
        //        firstRow = gridMain.GetDataSourceRowIndex(row);
        //        dRow = dt.Rows[firstRow];
        //        dx.ImportRow(dRow);
        //    }

        //    this.Cursor = Cursors.WaitCursor;

        //    barImport.Show();
        //    barImport.Minimum = 0;
        //    barImport.Maximum = dx.Rows.Count;
        //    barImport.Refresh();

        //    iTextSharp.text.Document sourceDocument = null;
        //    PdfCopy pdfCopyProvider = null;
        //    PdfImportedPage importedPage;

        //    string outputPdfPath = @"C:/rag/pdfX.pdf";
        //    GrantAccess(@"C:/rag");
        //    GrantFileAccess(outputPdfPath);

        //    if (File.Exists(outputPdfPath))
        //    {
        //        File.SetAttributes(outputPdfPath, FileAttributes.Normal);
        //        File.Delete(outputPdfPath);
        //    }

        //    sourceDocument = new iTextSharp.text.Document();
        //    pdfCopyProvider = new PdfCopy(sourceDocument, new System.IO.FileStream(outputPdfPath, System.IO.FileMode.Create));

        //    //output file Open  
        //    sourceDocument.Open();

        //    string contract = "";
        //    string record = "";
        //    for (int i = 0; i < dx.Rows.Count; i++)
        //    {
        //        barImport.Value = i + 1;
        //        barImport.Refresh();
        //        contract = dx.Rows[i]["contractNumber"].ObjToString();
        //        if (String.IsNullOrWhiteSpace(contract))
        //            return;

        //        record = dx.Rows[i]["record"].ObjToString();
        //        if (String.IsNullOrWhiteSpace(record))
        //            return;


        //        string historyFile = @"C:/rag/pdfDaily.pdf";
        //        GrantFileAccess(historyFile);
        //        DailyHistory histForm = new DailyHistory(contract, historyFile, true);

        //        workPDFfile = @"c:/rag/workPDF1.pdf";


        //        string manualFile = @"c:/rag/Manual.pdf";
        //        string payOffFile = @"c:/rag/ForcePayoff.pdf";

        //        GrantFileAccess(manualFile);
        //        GrantFileAccess(payOffFile);


        //        ForcedPayoffs forceForm = new ForcedPayoffs(true, record, this.dateTimePicker1.Value, this.dateTimePicker2.Value);

        //        MergeAllPDF(pdfCopyProvider, payOffFile, manualFile, historyFile);

        //        if (File.Exists(payOffFile))
        //        {
        //            File.SetAttributes(payOffFile, FileAttributes.Normal);
        //            File.Delete(payOffFile);
        //        }

        //        if (File.Exists(historyFile))
        //        {
        //            File.SetAttributes(historyFile, FileAttributes.Normal);
        //            File.Delete(historyFile);
        //        }

        //        if (File.Exists(manualFile))
        //        {
        //            File.SetAttributes(manualFile, FileAttributes.Normal);
        //            File.Delete(manualFile);
        //        }
        //    }

        //    //save the output file  
        //    sourceDocument.Close();

        //    barImport.Value = dx.Rows.Count;
        //    barImport.Refresh();

        //    ViewPDF myView = new ViewPDF("TEST", outputPdfPath);
        //    myView.ShowDialog();

        //    if (File.Exists(outputPdfPath))
        //    {
        //        File.SetAttributes(outputPdfPath, FileAttributes.Normal);
        //        File.Delete(outputPdfPath);
        //    }

        //    this.Cursor = Cursors.Default;
        //    dx.Dispose();

        //    barImport.Hide();
        //}
        /***********************************************************************************************/
        private static void MergeAllPDF(PdfCopy pdfCopyProvider, string File1, string File2, string File3, string File4 )
        {
            string[] fileArray = new string[5];
            fileArray[0] = File1;
            fileArray[1] = File2;
            fileArray[2] = File3;
            fileArray[3] = File4;

            PdfReader reader = null;
            PdfImportedPage importedPage;


            //files list wise Loop  
            for (int f = 0; f < fileArray.Length - 1; f++)
            {
                int pages = TotalPageCount(fileArray[f]);

                reader = new PdfReader(fileArray[f]);
                //Add pages in new file  
                for (int i = 1; i <= pages; i++)
                {
                    importedPage = pdfCopyProvider.GetImportedPage(reader, i);
                    pdfCopyProvider.AddPage(importedPage);
                }

                reader.Close();
            }
        }
        /***********************************************************************************************/
        private static int TotalPageCount(string file)
        {
            if (File.Exists(file))
            {
                using (StreamReader sr = new StreamReader(System.IO.File.OpenRead(file)))
                {
                    Regex regex = new Regex(@"/Type\s*/Page[^s]");
                    MatchCollection matches = regex.Matches(sr.ReadToEnd());

                    return matches.Count;
                }
            }
            else
                return 0;
        }
        /****************************************************************************************/
    }
}