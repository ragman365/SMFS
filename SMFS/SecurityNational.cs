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
using DevExpress.Utils;
using DevExpress.Office.Utils;
using DevExpress.DirectWrite;
using DevExpress.XtraPrinting.Native;
using DevExpress.Utils.DPI;
using DevExpress.Charts.Native;
using DevExpress.CodeParser;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraPrinting;
/****************************************************************************************/
namespace SMFS
{
    /****************************************************************************************/
    public partial class SecurityNational : DevExpress.XtraEditors.XtraForm
    {
        private DataTable workDt = null;
        private DataTable originalDt = null;
        private string mainFile = "";
        private string lastNameFound = "";
        /****************************************************************************************/
        public SecurityNational()
        {
            InitializeComponent();
        }
        /****************************************************************************************/
        private void SecurityNational_Load(object sender, EventArgs e)
        {
            btnVerifyData.Hide();
            btnVerifyTotals.Hide();
            labelMaximum.Hide();
            barImport.Hide();

            LoadFilterBy();
            G1.SetupVisibleColumns(gridMain3, this.columnsToolStripMenuItem, nmenu_Click);
        }
        /***********************************************************************************************/
        void nmenu_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem menu = (ToolStripMenuItem)sender;
            string name = menu.Name;
            int index = getGridColumnIndex(name);
            if (index < 0)
                return;
            if (menu.Checked)
            {
                menu.Checked = false;
                gridMain.Columns[index].Visible = false;
            }
            else
            {
                menu.Checked = true;
                gridMain.Columns[index].Visible = true;
            }
            gridMain.RefreshData();
            dgv.Refresh();
            ToolStripMenuItem xmenu = this.columnsToolStripMenuItem;
            xmenu.ShowDropDown();
        }
        /***********************************************************************************************/
        private int getGridColumnIndex(string columnName)
        {
            int index = -1;
            for (int i = 0; i < gridMain.Columns.Count; i++)
            {
                string name = gridMain.Columns[i].Name;
                if (name == columnName)
                {
                    index = i;
                    break;
                }
            }
            return index;
        }
        /****************************************************************************************/
        private double originalSize = 0D;
        private Font mainFont = null;
        private void ScaleCells()
        {
            if (originalSize == 0D)
            {
                //                originalSize = gridMain.Columns["address1"].AppearanceCell.FontSizeDelta.ObjToDouble();
                originalSize = gridMain3.Columns["address"].AppearanceCell.Font.Size;
                mainFont = gridMain3.Columns["address"].AppearanceCell.Font;
            }
            double scale = txtScale.Text.ObjToDouble();
            double size = scale / 100D * originalSize;
            Font font = new Font(mainFont.Name, (float)size);
            for (int i = 0; i < gridMain3.Columns.Count; i++)
            {
                gridMain3.Columns[i].AppearanceCell.Font = font;
            }
            gridMain3.RefreshData();
            dgv3.Refresh();
            this.Refresh();
        }
        /****************************************************************************************/
        private void txtScale_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                string balance = txtScale.Text.Trim();
                if (!G1.validate_numeric(balance))
                {
                    MessageBox.Show("***ERROR*** Scale must be numeric!");
                    return;
                }
                double money = balance.ObjToDouble();
                balance = G1.ReformatMoney(money);
                txtScale.Text = balance;
                ScaleCells();
                return;
            }
            // Initialize the flag to false.
            bool nonNumberEntered = false;

            // Determine whether the keystroke is a number from the top of the keyboard.
            if (e.KeyCode < Keys.D0 || e.KeyCode > Keys.D9)
            {
                // Determine whether the keystroke is a number from the keypad.
                if (e.KeyCode < Keys.NumPad0 || e.KeyCode > Keys.NumPad9)
                {
                    // Determine whether the keystroke is a backspace.
                    if (e.KeyCode != Keys.Back)
                    {
                        // A non-numerical keystroke was pressed.
                        // Set the flag to true and evaluate in KeyPress event.
                        if (e.KeyCode != Keys.OemPeriod)
                            nonNumberEntered = true;
                    }
                }
            }
            //If shift key was pressed, it's not a number.
            if (Control.ModifierKeys == Keys.Shift)
            {
                nonNumberEntered = true;
            }
            if (nonNumberEntered)
            {
                MessageBox.Show("***ERROR*** Key entered must be a number!");
                e.Handled = true;
            }
        }
        /****************************************************************************************/
        private void btnVerifyTotals_Click(object sender, EventArgs e)
        {
            if (dgv == null)
                return;
            DataTable dt = (DataTable)dgv.DataSource;
            if (dt == null)
                return;
            double m_DeathClaim = 0D;
            double m_Error = 0D;
            double m_ETI = 0D;
            double m_Expired = 0D;
            double m_Lapsed = 0D;
            double m_Matured = 0D;
            double m_PaidUp = 0D;
            double m_PremPaying = 0D;
            double m_RPU = 0D;
            double m_Surrender = 0D;
            double m_TerminatedAtIssue = 0D;
            double m_Total = 0D;

            double f_DeathClaim = 0D;
            double f_Error = 0D;
            double f_ETI = 0D;
            double f_Expired = 0D;
            double f_Lapsed = 0D;
            double f_Matured = 0D;
            double f_PaidUp = 0D;
            double f_PremPaying = 0D;
            double f_RPU = 0D;
            double f_Surrender = 0D;
            double f_TerminatedAtIssue = 0D;
            double f_Total = 0D;

            double c_DeathClaim = 0D;
            double c_Error = 0D;
            double c_ETI = 0D;
            double c_Expired = 0D;
            double c_Lapsed = 0D;
            double c_Matured = 0D;
            double c_PaidUp = 0D;
            double c_PremPaying = 0D;
            double c_RPU = 0D;
            double c_Surrender = 0D;
            double c_TerminatedAtIssue = 0D;
            double c_Total = 0D;

            string status = "";
            double monthly = 0D;
            double face = 0D;

            this.Cursor = Cursors.WaitCursor;

            try
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    status = dt.Rows[i]["STATUS"].ObjToString().Trim().ToUpper();
                    if (!String.IsNullOrWhiteSpace(status))
                    {
                        monthly = dt.Rows[i]["monthly"].ObjToDouble();
                        face = dt.Rows[i]["face"].ObjToDouble();
                        if (status == "EXPIRED")
                        {
                            m_Expired += monthly;
                            f_Expired += face;
                            c_Expired++;
                        }
                        else if (status == "DEATHCLAIM")
                        {
                            m_DeathClaim += monthly;
                            f_DeathClaim += face;
                            c_DeathClaim++;
                        }
                        else if (status == "ERROR")
                        {
                            m_Error += monthly;
                            f_Error += face;
                            c_Error++;
                        }
                        else if (status == "ETI")
                        {
                            m_ETI += monthly;
                            f_ETI += face;
                            c_ETI++;
                        }
                        else if (status == "LAPSED")
                        {
                            m_Lapsed += monthly;
                            f_Lapsed += face;
                            c_Lapsed++;
                        }
                        else if (status == "MATURED")
                        {
                            m_Matured += monthly;
                            f_Matured += face;
                            c_Matured++;
                        }
                        else if (status == "PAIDUP")
                        {
                            m_PaidUp += monthly;
                            f_PaidUp += face;
                            c_PaidUp++;
                        }
                        else if (status == "PREMPAYING")
                        {
                            m_PremPaying += monthly;
                            f_PremPaying += face;
                            c_PremPaying++;
                        }
                        else if (status == "RPU")
                        {
                            m_RPU += monthly;
                            f_RPU += face;
                            c_RPU++;
                        }
                        else if (status == "SURRENDER")
                        {
                            m_Surrender += monthly;
                            f_Surrender += face;
                            c_Surrender++;
                        }
                        else if (status == "TERMINATEDATISSUE")
                        {
                            m_TerminatedAtIssue += monthly;
                            f_TerminatedAtIssue += face;
                            c_TerminatedAtIssue++;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }

            m_Total = m_DeathClaim + m_Error + m_ETI + m_Expired + m_Lapsed + m_Matured + m_PaidUp + m_PremPaying + m_RPU + m_Surrender + m_TerminatedAtIssue;
            f_Total = f_DeathClaim + f_Error + f_ETI + f_Expired + f_Lapsed + f_Matured + f_PaidUp + f_PremPaying + f_RPU + f_Surrender + f_TerminatedAtIssue;
            c_Total = c_DeathClaim + c_Error + c_ETI + c_Expired + c_Lapsed + c_Matured + c_PaidUp + c_PremPaying + c_RPU + c_Surrender + c_TerminatedAtIssue;

            this.Cursor = Cursors.Default;

            DataTable dx = new DataTable();
            dx.Columns.Add("num");
            dx.Columns.Add("category");
            dx.Columns.Add("monthlyPremium", Type.GetType("System.Double"));
            dx.Columns.Add("faceAmount", Type.GetType("System.Double"));
            dx.Columns.Add("count", Type.GetType("System.Double"));

            AddCategory(dx, "DeathClaim", m_DeathClaim, f_DeathClaim, c_DeathClaim);
            AddCategory(dx, "Error", m_Error, f_Error, c_Error);
            AddCategory(dx, "ETI", m_ETI, f_ETI, c_ETI);
            AddCategory(dx, "Expired", m_Expired, f_Expired, c_Expired);
            AddCategory(dx, "Lapsed", m_Lapsed, f_Lapsed, c_Lapsed);
            AddCategory(dx, "Matured", m_Matured, f_Matured, c_Matured);
            AddCategory(dx, "PaidUp", m_PaidUp, f_PaidUp, c_PaidUp);
            AddCategory(dx, "PremPaying", m_PremPaying, f_PremPaying, c_PremPaying);
            AddCategory(dx, "RPU", m_RPU, f_RPU, c_RPU);
            AddCategory(dx, "Surrender", m_Surrender, f_Surrender, c_Surrender);
            AddCategory(dx, "TerminatedAtIssue", m_TerminatedAtIssue, f_TerminatedAtIssue, c_TerminatedAtIssue);
            AddCategory(dx, "Total", m_Total, f_Total, c_Total);

            G1.NumberDataTable(dx);
            dgv2.DataSource = dx;
        }
        /****************************************************************************************/
        private void AddCategory(DataTable dx, string category, double monthlyPremium, double faceAmount, double count)
        {
            try
            {
                DataRow dRow = dx.NewRow();
                dRow["category"] = category;
                dRow["monthlyPremium"] = monthlyPremium;
                dRow["faceAmount"] = faceAmount;
                dRow["count"] = count;
                dx.Rows.Add(dRow);
            }
            catch (Exception ex)
            {
            }
        }
        /****************************************************************************************/
        private void btnVerifyData_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;

            DataTable dx = new DataTable();
            dx.Columns.Add("num");
            dx.Columns.Add("match");
            dx.Columns.Add("found");
            dx.Columns.Add("family");
            dx.Columns.Add("payer");
            dx.Columns.Add("smfsPayer");
            dx.Columns.Add("policy");
            dx.Columns.Add("smfsPolicy");
            dx.Columns.Add("status");
            dx.Columns.Add("smfsStatus");
            dx.Columns.Add("name");
            dx.Columns.Add("smfsName");
            dx.Columns.Add("issueDate");
            dx.Columns.Add("smfsIssueDate");
            dx.Columns.Add("paidTo");
            dx.Columns.Add("smfsPaidTo");
            dx.Columns.Add("birthDate");
            dx.Columns.Add("smfsBirthDate");
            dx.Columns.Add("face", Type.GetType("System.Double"));
            dx.Columns.Add("smfsFace", Type.GetType("System.Double"));
            dx.Columns.Add("monthly", Type.GetType("System.Double"));
            dx.Columns.Add("smfsMonthly", Type.GetType("System.Double"));
            dx.Columns.Add("address");
            dx.Columns.Add("smfsAddress");

            string family = "";
            string payer = "";
            string policy = "";
            string status = "";
            string name = "";
            double face = 0D;
            string paidTo = "";
            double monthly = 0D;
            string issue = "";
            string birthDate = "";
            string address = "";

            string smfs_family = "";
            string smfs_payer = "";
            string smfs_policy = "";
            string smfs_status = "";
            string smfs_name = "";
            double smfs_face = 0D;
            string smfs_paidTo = "";
            double smfs_monthly = 0D;
            string smfs_issue = "";
            string smfs_birthDate = "";
            string smfs_address = "";

            string payerLookup = "";

            string address1 = "";
            string address2 = "";
            string city = "";
            string state = "";
            string zip1 = "";
            string zip2 = "";


            string first = "";
            string last = "";
            bool rv = false;
            string cmd = "";
            string str = "";
            DateTime date = DateTime.Now;
            string issueDate = "";
            string premium = "";

            string match = "";
            string contractNumber = "";
            DataTable cDt = null;

            DataRow dRow = null;
            DataTable pDt = null;

            DateTime deceasedDate = DateTime.Now;
            DateTime lapseDate = DateTime.Now;
            string lapsed = "";

            int i = 0;

            labelMaximum.Text = "";
            labelMaximum.Show();
            barImport.Show();

            string found = "";

            int lastRow = dt.Rows.Count;
            //lastRow = 2000;
            if (dt.Rows.Count < lastRow)
                lastRow = dt.Rows.Count;
            barImport.Minimum = 0;
            barImport.Maximum = lastRow;

            lastNameFound = "";
            int myRow = 0;

            try
            {
                for (i = 1; i < lastRow; i++)
                {
                    Application.DoEvents();
                    barImport.Value = i + 1;
                    barImport.Refresh();
                    labelMaximum.Text = (i + 1).ToString() + " of " + lastRow.ToString();
                    labelMaximum.Refresh();

                    found = "NO";
                    match = "";
                    str = dt.Rows[i]["family"].ObjToString();
                    if (!String.IsNullOrWhiteSpace(str))
                    {
                        family = str;
                        payer = dt.Rows[i]["name"].ObjToString();
                        address = dt.Rows[i]["face"].ObjToString();
                    }
                    else
                    {
                        name = dt.Rows[i]["name"].ObjToString();
                        if (name.Trim().ToUpper().IndexOf("COUNT") == 0)
                            continue;
                        if (String.IsNullOrWhiteSpace(name))
                            continue;
                        policy = dt.Rows[i]["policy"].ObjToString();
                        status = dt.Rows[i]["status"].ObjToString();
                        monthly = dt.Rows[i]["monthly"].ObjToDouble();
                        face = dt.Rows[i]["face"].ObjToDouble();
                        paidTo = dt.Rows[i]["paid To"].ObjToString();
                        issue = dt.Rows[i]["issue"].ObjToString();
                        birthDate = dt.Rows[i]["Birth Date"].ObjToString();

                        pDt = FindPolicy(name, issue, birthDate, paidTo, monthly, face);

                        try
                        {
                            dRow = dx.NewRow();
                            dRow["family"] = family;
                            dRow["payer"] = payer;
                            dRow["policy"] = policy;
                            dRow["status"] = status;
                            dRow["name"] = name;
                            dRow["monthly"] = monthly;
                            dRow["face"] = face;
                            dRow["paidTo"] = paidTo;
                            dRow["issueDate"] = issue;
                            dRow["birthDate"] = birthDate;
                            dRow["address"] = address;
                            if (pDt != null)
                            {
                                if (pDt.Rows.Count > 1)
                                    pDt = BreakDownPolicies(pDt, issue, birthDate, paidTo, monthly, face);
                                if (pDt.Rows.Count <= 0)
                                    pDt = DetailSearch(name, issue, birthDate, paidTo, monthly, face);
                                if (pDt.Rows.Count <= 0)
                                {
                                    pDt = SearchNames(pDt, name, issue, paidTo, birthDate, monthly, face);
                                }

                                if (pDt.Rows.Count > 0)
                                {
                                    myRow = 0;
                                    smfs_name = pDt.Rows[myRow]["policyLastName"].ObjToString() + " " + pDt.Rows[0]["policyFirstName"].ObjToString();
                                    if (smfs_name == lastNameFound)
                                    {
                                        if (pDt.Rows.Count > 1)
                                            myRow = 1;
                                    }

                                    date = pDt.Rows[myRow]["issueDate8"].ObjToDateTime();
                                    smfs_issue = date.ToString("MM/dd/yyyy");

                                    date = pDt.Rows[myRow]["birthDate"].ObjToDateTime();
                                    smfs_birthDate = date.ToString("MM/dd/yyyy");
                                    if (smfs_birthDate != birthDate)
                                        match += "B";

                                    smfs_monthly = pDt.Rows[myRow]["premium"].ObjToDouble();
                                    if (smfs_monthly != monthly)
                                        match += "M";

                                    smfs_face = pDt.Rows[myRow]["liability"].ObjToDouble();
                                    if (smfs_face != face)
                                        match += "F";

                                    dRow["smfsPolicy"] = pDt.Rows[myRow]["policyNumber"].ObjToString();
                                    dRow["smfsIssueDate"] = smfs_issue;
                                    dRow["smfsBirthDate"] = smfs_birthDate;
                                    dRow["smfsMonthly"] = smfs_monthly;
                                    dRow["smfsFace"] = smfs_face;
                                    dRow["smfsName"] = smfs_name;
                                    dRow["smfsPayer"] = pDt.Rows[myRow]["payer"].ObjToString();

                                    lastNameFound = smfs_name;

                                    deceasedDate = pDt.Rows[myRow]["deceasedDate"].ObjToDateTime();
                                    if (deceasedDate.Year > 100)
                                        dRow["smfsStatus"] = "Expired";
                                    else
                                    {
                                        lapseDate = pDt.Rows[myRow]["lapsedDate8"].ObjToDateTime();
                                        lapsed = pDt.Rows[myRow]["lapsed"].ObjToString().ToUpper();
                                        if (lapseDate.Year > 100 || lapsed == "Y")
                                            dRow["smfsStatus"] = "Lapsed";

                                    }

                                    if (dRow["smfsStatus"].ObjToString().ToUpper() != status.ToUpper())
                                        match += "S";

                                    contractNumber = pDt.Rows[myRow]["contractNumber"].ObjToString();

                                    cmd = "Select * from `icontracts` p JOIN `icustomers` c ON p.`contractNumber` = c.`contractNumber` where p.`contractNumber` = '" + contractNumber + "';";
                                    cDt = G1.get_db_data(cmd);
                                    if (cDt.Rows.Count > 0)
                                    {
                                        smfs_paidTo = cDt.Rows[0]["lastDatePaid8"].ObjToDateTime().ToString("M/d/yyyy");
                                        payerLookup = cDt.Rows[0]["payer"].ObjToString();
                                        smfs_paidTo = GetPaidToDate(payerLookup);
                                        dRow["smfsPaidTo"] = smfs_paidTo;
                                        address1 = cDt.Rows[0]["address1"].ObjToString();
                                        address2 = cDt.Rows[0]["address2"].ObjToString();
                                        city = cDt.Rows[0]["city"].ObjToString();
                                        state = cDt.Rows[0]["state"].ObjToString();
                                        zip1 = cDt.Rows[0]["zip1"].ObjToString();
                                        zip2 = cDt.Rows[0]["zip2"].ObjToString();
                                        smfs_address = address1;
                                        if (!String.IsNullOrWhiteSpace(address2))
                                            smfs_address += " " + address2;
                                        if (!String.IsNullOrWhiteSpace(city))
                                            smfs_address += " " + city;
                                        if (!String.IsNullOrWhiteSpace(state))
                                            smfs_address += " " + state;
                                        if (!String.IsNullOrWhiteSpace(zip1))
                                            smfs_address += " " + zip1;
                                        if (!String.IsNullOrWhiteSpace(zip2))
                                            smfs_address += " " + zip2;
                                        dRow["smfsAddress"] = smfs_address;
                                    }
                                    found = "YES";
                                }
                            }
                            dRow["found"] = found;
                            dRow["match"] = match;
                            dx.Rows.Add(dRow);
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }

            G1.NumberDataTable(dx);
            originalDt = dx;
            dgv3.DataSource = dx;
            tabControl1.SelectTab("tabDetail");
            G1.SetupVisibleColumns(gridMain3, this.columnsToolStripMenuItem, nmenu_Click);
            ScaleCells();
        }
        /****************************************************************************************/
        private DataTable SearchNames(DataTable pDt, string name, string issue, string paidTo, string birth, double monthly, double face)
        {
            DateTime date = issue.ObjToDateTime();
            string issueDate = date.ToString("yyyy-MM-dd");
            string first = "";
            string last = "";
            string cmd = "";

            name = name.Replace("-", " ");

            string[] Lines = name.Split(' ');

            if (Lines.Length <= 0)
                return pDt;

            if (pDt == null)
                pDt = G1.get_db_data("Select * from `policies` where `record` = '-999';");

            DataTable dt = null;

            last = Lines[0];
            first = "";

            for (int i = Lines.Length - 1; i > 0; i--)
            {
                if (!String.IsNullOrWhiteSpace(first))
                    first = " " + first;
                first = Lines[i].Trim() + first;
            }

            cmd = "Select * from `policies` where `policyFirstName` = '" + first + "' and `policyLastName` = '" + last + "' ";
            cmd += ";"; // Try Exact Match
            dt = G1.get_db_data(cmd);
            if (dt.Rows.Count > 0)
            {
                for (int j = 0; j < dt.Rows.Count; j++)
                    pDt.ImportRow(dt.Rows[j]);
            }
            if (pDt.Rows.Count > 0)
                return pDt;

            for (int i = Lines.Length - 1; i > 0; i--)
            {
                first = Lines[i].Trim();
                cmd = "Select * from `policies` where `policyFirstName` LIKE '%" + first + "%' and `policyLastName` = '" + last + "' ";
                cmd += ";";
                dt = G1.get_db_data(cmd);
                if (dt.Rows.Count > 0)
                {
                    for (int j = 0; j < dt.Rows.Count; j++)
                        pDt.ImportRow(dt.Rows[j]);
                }
            }

            int lastRow = Lines.Length - 1;
            last = Lines[lastRow].Trim();
            for (int i = Lines.Length - 2; i >= 0; i--)
            {
                first = Lines[i].Trim();
                cmd = "Select * from `policies` where `policyFirstName` LIKE '%" + first + "%' and `policyLastName` = '" + last + "' ";
                cmd += ";";
                dt = G1.get_db_data(cmd);
                if (dt.Rows.Count > 0)
                {
                    for (int j = 0; j < dt.Rows.Count; j++)
                        pDt.ImportRow(dt.Rows[j]);
                }
            }

            return pDt;
        }
        /****************************************************************************************/
        private DataTable BreakDownPolicies(DataTable pDt, string issue, string paidTo, string birth, double monthly, double face)
        {
            DataRow[] dRows = null;
            DataTable dt = pDt.Clone();
            DataTable xDt = null;
            string str = "";
            string cmd = "";

            DateTime date = DateTime.Now;

            try
            {
                if (!String.IsNullOrWhiteSpace(issue))
                {
                    DateTime issueDate = issue.ObjToDateTime();
                    for (int i = 0; i < pDt.Rows.Count; i++)
                    {
                        date = pDt.Rows[i]["issueDate8"].ObjToDateTime();
                        if (date == issueDate)
                            dt.ImportRow(pDt.Rows[i]);
                    }
                }
                if (dt.Rows.Count > 1)
                {
                    str = G1.ReformatMoney(monthly);
                    str = str.Replace(",", "");
                    dRows = dt.Select("premium='" + str + "'");
                    if (dRows.Length > 0)
                    {
                        for (int i = 0; i < dRows.Length; i++)
                            dt.ImportRow(dRows[i]);
                    }
                }
                if (dt.Rows.Count <= 0)
                {
                    cmd = "Select * from `policies` where `premium` = '" + monthly.ToString() + "' AND `liability` = '" + face.ToString() + "';";
                    xDt = G1.get_db_data(cmd);
                    DateTime issueDate = issue.ObjToDateTime();
                    for (int i = 0; i < xDt.Rows.Count; i++)
                    {
                        date = xDt.Rows[i]["issueDate8"].ObjToDateTime();
                        if (date == issueDate)
                            dt.ImportRow(xDt.Rows[i]);
                    }
                }
            }
            catch (Exception ex)
            {
            }
            if (dt != null)
                return dt;
            return pDt;
        }
        /****************************************************************************************/
        private DataTable DetailSearch(string name, string issue, string birth, string paidTo, double monthly, double face)
        {
            string cmd = "Select * from `policies` where `premium` = '" + monthly.ToString() + "' AND `liability` = '" + face.ToString() + "';";
            DataTable xDt = G1.get_db_data(cmd);
            DataTable dt = xDt.Clone();
            DateTime issueDate = issue.ObjToDateTime();
            DateTime date = DateTime.Now;
            for (int i = 0; i < xDt.Rows.Count; i++)
            {
                date = xDt.Rows[i]["issueDate8"].ObjToDateTime();
                if (date == issueDate)
                    dt.ImportRow(xDt.Rows[i]);
            }
            if (dt.Rows.Count <= 1)
                return dt;
            xDt = dt.Copy();
            //DataTable nDt = dt.Clone();
            DataTable ddt = null;
            date = paidTo.ObjToDateTime();
            string lpd = date.ToString("yyyy-MM-dd");
            string contractNumber = "";
            dt.Rows.Clear();
            try
            {
                for (int i = 0; i < xDt.Rows.Count; i++)
                {
                    contractNumber = xDt.Rows[i]["contractNumber"].ObjToString();
                    cmd = "Select * from `icontracts` WHERE `contractNumber` = '" + contractNumber + "' AND `lastDatePaid8` = '" + lpd + "';";
                    ddt = G1.get_db_data(cmd);
                    if (ddt.Rows.Count > 0)
                    {
                        dt.ImportRow(xDt.Rows[i]);
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return dt;
        }
        /****************************************************************************************/
        private DataTable FindPolicy(string name, string issue, string birth, string paidTo, double monthly, double face)
        {
            DateTime date = issue.ObjToDateTime();
            string issueDate = date.ToString("yyyy-MM-dd");
            string first = "";
            string last = "";
            string cmd = "";

            DataTable pDt = null;
            string[] Lines = name.Split(' ');

            if (Lines.Length <= 0)
                return pDt;

            last = Lines[0];

            for (int i = Lines.Length - 1; i > 0; i--)
            {
                first = Lines[i].Trim();
                cmd = "Select * from `policies` where `policyFirstName` LIKE '%" + first + "%' and `policyLastName` = '" + last + "' ";
                cmd += ";";
                pDt = G1.get_db_data(cmd);
                if (pDt.Rows.Count > 0)
                    return pDt;
            }

            int lastRow = Lines.Length - 1;
            last = Lines[lastRow].Trim();
            for (int i = Lines.Length - 2; i > 0; i--)
            {
                first = Lines[i].Trim();
                cmd = "Select * from `policies` where `policyFirstName` LIKE '%" + first + "%' and `policyLastName` = '" + last + "' ";
                cmd += ";";
                pDt = G1.get_db_data(cmd);
                if (pDt.Rows.Count > 0)
                    break;
            }

            return pDt;
        }
        /****************************************************************************************/
        private bool ParseOutName(string name, ref string first, ref string last)
        {
            first = "";
            last = "";
            bool rv = false;
            string[] Lines = name.Split(' ');
            if (Lines.Length >= 2)
            {
                last = Lines[0].Trim();
                int l = Lines.Length - 1;
                first = Lines[l].Trim();
                rv = true;
            }
            return rv;
        }
        /****************************************************************************************/
        private void chkFilterBy_EditValueChanged(object sender, EventArgs e)
        {

        }
        /*******************************************************************************************/
        private string getFilterQuery(string agent = "")
        {
            string procLoc = "";
            string[] locIDs = this.chkFilterBy.EditValue.ToString().Split('|');
            for (int i = 0; i < locIDs.Length; i++)
            {
                if (!String.IsNullOrWhiteSpace(locIDs[i]))
                {
                    if (procLoc.Trim().Length > 0)
                        procLoc += ",";
                    procLoc += "'" + locIDs[i].Trim() + "'";
                }
            }
            if (String.IsNullOrWhiteSpace(agent))
                agent = "match";
            return procLoc.Length > 0 ? " `" + agent + "` CONTAINS (" + procLoc + ") " : "";
        }
        /****************************************************************************************/
        private void LoadFilterBy()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Filter");
            AddFilterRow(dt, "BirthDate");
            AddFilterRow(dt, "Monthly");
            AddFilterRow(dt, "Face");
            AddFilterRow(dt, "Status");
            AddFilterRow(dt, "Not Found");
            chkFilterBy.Properties.DataSource = dt;
        }
        /****************************************************************************************/
        private void AddFilterRow(DataTable dt, string filter)
        {
            DataRow dRow = dt.NewRow();
            dRow["Filter"] = filter;
            dt.Rows.Add(dRow);
        }
        /****************************************************************************************/
        private void gridMain_CustomRowFilter(object sender, DevExpress.XtraGrid.Views.Base.RowFilterEventArgs e)
        {
        }
        /****************************************************************************************/
        private void chkFilterBy_EditValueChanged_1(object sender, EventArgs e)
        {
            gridMain3.RefreshData();
            dgv3.Refresh();
        }
        /****************************************************************************************/
        private void gridMain3_CustomRowFilter(object sender, DevExpress.XtraGrid.Views.Base.RowFilterEventArgs e)
        {
            if (String.IsNullOrWhiteSpace(chkFilterBy.Text))
                return;
            string str = chkFilterBy.Text;
            int row = e.ListSourceRow;
            DataTable dt = (DataTable)dgv3.DataSource;
            string data = dt.Rows[row]["match"].ObjToString();
            string myfound = dt.Rows[row]["found"].ObjToString().Trim().ToUpper();
            string[] Lines = str.Split('|');
            string what = "";
            bool found = false;
            for (int i = 0; i < Lines.Length; i++)
            {
                what = Lines[i].Trim();
                if (String.IsNullOrWhiteSpace(what))
                    continue;
                if (what.ToUpper() == "NOT FOUND" && myfound == "NO")
                {
                    found = true;
                    break;
                }
                else if (what.ToUpper() == "BIRTHDATE" && data.Contains("B"))
                {
                    found = true;
                    break;
                }
                else if (what.ToUpper() == "MONTHLY" && data.Contains("M"))
                {
                    found = true;
                    break;
                }
                if (what.ToUpper() == "FACE" && data.Contains("F"))
                {
                    found = true;
                    break;
                }
                if (what.ToUpper() == "STATUS" && data.Contains("S"))
                {
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                e.Visible = false;
                e.Handled = true;
                return;
            }
        }
        /****************************************************************************************/
        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrWhiteSpace(mainFile))
            {
                saveAsToolStripMenuItem_Click(null, null);
                return;
            }

            DataTable dt = (DataTable)dgv3.DataSource;
            if (dt == null)
                return;
            if (dt.Rows.Count <= 0)
                return;

            MySQL.CreateCSVfile(dt, mainFile, true, "~");
        }
        /****************************************************************************************/
        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv3.DataSource;
            if (dt == null)
                return;
            if (dt.Rows.Count <= 0)
                return;

            string filter = "CSV files (*.csv)|*.csv";
            saveFileDialog1.Filter += filter;
            saveFileDialog1.FilterIndex = 0;
            saveFileDialog1.FileName = "";
            if (saveFileDialog1.ShowDialog() != DialogResult.OK)
                return;

            string filename = saveFileDialog1.FileName;

            DataTable tempDt = dt.Copy();
            tempDt.Columns.RemoveAt(0);

            MySQL.CreateCSVfile(tempDt, filename, true, "~");
            mainFile = filename;
        }
        /****************************************************************************************/
        private void menuReadFile_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string file = ofd.FileName;
                    DataTable dt = Import.ImportCSVfile(file, null, false, "~");
                    dt.Columns["Num"].ColumnName = "num";
                    G1.NumberDataTable(dt);
                    dgv3.DataSource = dt;
                    ScaleCells();
                    G1.SetupVisibleColumns(gridMain3, this.columnsToolStripMenuItem, nmenu_Click);
                    tabControl1.SelectTab("tabDetail");
                }
            }
        }
        /****************************************************************************************/
        private void btnImport_Click(object sender, EventArgs e)
        {
            Import importForm = new Import();
            importForm.SelectDone += ImportForm_SelectDone6;
            importForm.Text = "Import Sec. Nat. Data File";
            importForm.Show();
        }
        /***********************************************************************************************/
        private void ImportForm_SelectDone6(DataTable dt)
        {
            if (dt == null)
                return;
            workDt = dt;
            btnVerifyData.Hide();
            btnVerifyTotals.Hide();
            labelMaximum.Hide();
            barImport.Hide();

            if (workDt != null)
            {
                string name = "";
                for (int i = 0; i < workDt.Columns.Count; i++)
                {
                    name = workDt.Columns[i].ColumnName.ObjToString();
                    G1.AddNewColumn(gridMain, name, name, "", FormatType.None, 25, true);
                }
                for (int i = 0; i < workDt.Columns.Count; i++)
                {
                    name = workDt.Columns[i].ColumnName.ObjToString();
                    G1.SetColumnPosition(gridMain, name, i);
                    G1.SetColumnWidth(gridMain, name, 75);
                }
                dgv.DataSource = workDt;
            }
            LoadFilterBy();
            btnVerifyTotals.Show();
            btnVerifyData.Show();
        }
        /****************************************************************************************/
        private void gridMain3_DoubleClick(object sender, EventArgs e)
        {
            FastLookup fastForm = new FastLookup();
            fastForm.ListDone += FastForm_ListDone;
            fastForm.Show();
        }
        /****************************************************************************************/
        private void FastForm_ListDone(string s)
        { // Trust or Policy Selected
            if (String.IsNullOrWhiteSpace(s))
                return;
            string[] Lines = s.Split(':');
            if (Lines.Length <= 1)
                return;
            string source = Lines[0].Trim();
            if (source.ToUpper() == "TRUST")
                return;
            string amount = "";
            if (Lines.Length >= 4)
                amount = Lines[3].Trim();
            string account = Lines[1].Trim();
            string name = "";
            if (Lines.Length >= 5)
                name = Lines[4].Trim();
            string policyRecord = "";
            for (int i = 0; i < Lines.Length; i++)
            {
                if (Lines[i].ToUpper().IndexOf("POLICYRECORD=") >= 0)
                {
                    string[] newLines = Lines[i].Split('=');
                    if (newLines.Length > 1)
                        policyRecord = newLines[1].Trim();
                }
            }
            if (String.IsNullOrWhiteSpace(policyRecord))
                return;
            string cmd = "Select * from `policies` where `record` = '" + policyRecord + "';";
            DataTable pDt = G1.get_db_data(cmd);
            if (pDt.Rows.Count <= 0)
                return;
            DataTable dt = (DataTable)dgv3.DataSource;
            int row = gridMain3.FocusedRowHandle;
            row = gridMain3.GetDataSourceRowIndex(row);

            string found = "NO";
            string match = "";
            string str = "";
            name = dt.Rows[row]["name"].ObjToString();
            string policy = dt.Rows[row]["policy"].ObjToString();
            string status = dt.Rows[row]["status"].ObjToString();
            double monthly = dt.Rows[row]["monthly"].ObjToDouble();
            double face = dt.Rows[row]["face"].ObjToDouble();
            string paidTo = dt.Rows[row]["paidTo"].ObjToString();
            string issue = dt.Rows[row]["issueDate"].ObjToString();
            string birthDate = dt.Rows[row]["birthDate"].ObjToString();

            try
            {
                DateTime date = pDt.Rows[0]["issueDate8"].ObjToDateTime();
                string smfs_issue = date.ToString("MM/dd/yyyy");

                date = pDt.Rows[0]["birthDate"].ObjToDateTime();
                string smfs_birthDate = date.ToString("MM/dd/yyyy");
                if (smfs_birthDate != birthDate)
                    match += "B";

                double smfs_monthly = pDt.Rows[0]["premium"].ObjToDouble();
                if (smfs_monthly != monthly)
                    match += "M";

                double smfs_face = pDt.Rows[0]["liability"].ObjToDouble();
                if (smfs_face != face)
                    match += "F";

                dt.Rows[row]["smfsPolicy"] = pDt.Rows[0]["policyNumber"].ObjToString();
                dt.Rows[row]["smfsIssueDate"] = smfs_issue;
                dt.Rows[row]["smfsBirthDate"] = smfs_birthDate;
                dt.Rows[row]["smfsMonthly"] = smfs_monthly;
                dt.Rows[row]["smfsFace"] = smfs_face;
                dt.Rows[row]["smfsName"] = pDt.Rows[0]["policyLastName"].ObjToString() + " " + pDt.Rows[0]["policyFirstName"].ObjToString();

                DateTime deceasedDate = pDt.Rows[0]["deceasedDate"].ObjToDateTime();
                if (deceasedDate.Year > 100)
                    dt.Rows[row]["smfsStatus"] = "Expired";
                else
                {
                    DateTime lapseDate = pDt.Rows[0]["lapsedDate8"].ObjToDateTime();
                    string lapsed = pDt.Rows[0]["lapsed"].ObjToString().ToUpper();
                    if (lapseDate.Year > 100 || lapsed == "Y")
                        dt.Rows[row]["smfsStatus"] = "Lapsed";

                }

                if (dt.Rows[row]["smfsStatus"].ObjToString().ToUpper() != status.ToUpper())
                    match += "S";

                string contractNumber = pDt.Rows[0]["contractNumber"].ObjToString();

                cmd = "Select * from `icontracts` p JOIN `icustomers` c ON p.`contractNumber` = c.`contractNumber` where p.`contractNumber` = '" + contractNumber + "';";
                DataTable cDt = G1.get_db_data(cmd);
                if (cDt.Rows.Count > 0)
                {
                    string smfs_paidTo = cDt.Rows[0]["lastDatePaid8"].ObjToDateTime().ToString("M/d/yyyy");
                    string payer = cDt.Rows[0]["payer"].ObjToString();
                    smfs_paidTo = GetPaidToDate(payer);
                    dt.Rows[row]["smfsPaidTo"] = smfs_paidTo;
                    string address1 = cDt.Rows[0]["address1"].ObjToString();
                    string address2 = cDt.Rows[0]["address2"].ObjToString();
                    string city = cDt.Rows[0]["city"].ObjToString();
                    string state = cDt.Rows[0]["state"].ObjToString();
                    string zip1 = cDt.Rows[0]["zip1"].ObjToString();
                    string zip2 = cDt.Rows[0]["zip2"].ObjToString();
                    string smfs_address = address1;
                    if (!String.IsNullOrWhiteSpace(address2))
                        smfs_address += " " + address2;
                    if (!String.IsNullOrWhiteSpace(city))
                        smfs_address += " " + city;
                    if (!String.IsNullOrWhiteSpace(state))
                        smfs_address += " " + state;
                    if (!String.IsNullOrWhiteSpace(zip1))
                        smfs_address += " " + zip1;
                    if (!String.IsNullOrWhiteSpace(zip2))
                        smfs_address += " " + zip2;
                    dt.Rows[row]["smfsAddress"] = smfs_address;
                }
                found = "YES";
                dt.Rows[row]["found"] = found;
                dt.Rows[row]["match"] = match;
                dgv3.DataSource = dt;
                gridMain.RefreshData();
                dgv3.Refresh();
                G1.SetupVisibleColumns(gridMain3, this.columnsToolStripMenuItem, nmenu_Click);
                ScaleCells();
            }
            catch (Exception ex)
            {
            }
        }
        /****************************************************************************************/
        private string GetPaidToDate ( string payer )
        {
            string dueDate = "";
            DateTime date = DateTime.Now;
            DateTime dueDate8 = DateTime.Now;
            bool first = true;
            string cmd = "Select * from `icontracts` p JOIN `icustomers` c ON p.`contractNumber` = c.`contractNumber` where c.`payer` = '" + payer + "' GROUP by c.`contractNumber`;";
            DataTable dt = G1.get_db_data(cmd);
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                date = dt.Rows[i]["dueDate8"].ObjToDateTime();
                if ( first )
                {
                    dueDate8 = date;
                    first = false;
                }
                if (date > dueDate8)
                    dueDate8 = date;
            }
            if ( !first)
                dueDate = dueDate8.ToString("M/d/yyyy");
            return dueDate;
        }
        /****************************************************************************************/
        private void gridMain3_CustomColumnDisplayText(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventArgs e)
        {
            ColumnView view = sender as ColumnView;
            bool doDate = false;
            if (e.Column.FieldName.ToUpper().IndexOf("DATE") >= 0 && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
                doDate = true;
            else if (e.Column.FieldName.ToUpper().IndexOf("PAIDTO") >= 0 && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
                doDate = true;
            if ( doDate )
            {
                if (e.DisplayText.IndexOf("0000") >= 0 || e.DisplayText.IndexOf("0001") >= 0)
                    e.DisplayText = "";
                else
                {
                    DateTime date = e.DisplayText.ObjToString().ObjToDateTime();
                    if (date.Year < 100)
                        e.DisplayText = "";
                    else
                        e.DisplayText = date.ToString("MM/dd/yyyy");
                }
            }
            else if (e.Column.FieldName.ToUpper() == "MATCH" && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
            {
                string data = e.DisplayText;
                int row = e.ListSourceRowIndex;
                DataTable dt = (DataTable)dgv3.DataSource;
                DateTime birthDate = dt.Rows[row]["birthDate"].ObjToDateTime();
                DateTime smfsBirthDate = dt.Rows[row]["smfsBirthDate"].ObjToDateTime();
                data = data.Replace("B", "");
                if (birthDate != smfsBirthDate)
                    data = "B" + data;
                e.DisplayText = data;
            }
        }
        /****************************************************************************************/
        private void gridMain3_CustomDrawCell(object sender, RowCellCustomDrawEventArgs e)
        {
            if (e.Column.FieldName.ToUpper() == "NUM")
            {
                if (e.RowHandle >= 0)
                {
                    string num = (e.RowHandle + 1).ToString();
                    e.DisplayText = num;
                }
            }
            else if (e.Column.FieldName.ToUpper() == "MATCH")
            {
                if ( e.RowHandle >= 0 )
                {
                    string data = e.DisplayText;
                    int row = gridMain3.GetDataSourceRowIndex(e.RowHandle);
                    DataTable dt = (DataTable)dgv3.DataSource;
                    DateTime birthDate = dt.Rows[row]["birthDate"].ObjToDateTime();
                    DateTime smfsBirthDate = dt.Rows[row]["smfsBirthDate"].ObjToDateTime();
                    data = data.Replace("B", "");
                    if (birthDate != smfsBirthDate)
                        data = "B" + data;
                    e.DisplayText = data;
                }
            }
        }
        /****************************************************************************************/
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            if (dgv.Visible)
                SetSpyGlass(gridMain);
            else if (dgv3.Visible)
                SetSpyGlass(gridMain3);
        }
        /***********************************************************************************************/
        private void SetSpyGlass(DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView grid)
        {
            if (grid.OptionsFind.AlwaysVisible == true)
                grid.OptionsFind.AlwaysVisible = false;
            else
                grid.OptionsFind.AlwaysVisible = true;
        }
        /***********************************************************************************************/
        private int pageMarginLeft = 0;
        private int pageMarginRight = 0;
        private int pageMarginTop = 0;
        private int pageMarginBottom = 0;
        private bool isPrinting = false;
        /***********************************************************************************************/
        private void printPreviewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            isPrinting = true;
            if (this.components == null)
                this.components = new System.ComponentModel.Container();

            DevExpress.XtraPrinting.PrintingSystem printingSystem1 = new DevExpress.XtraPrinting.PrintingSystem(this.components);
            DevExpress.XtraPrinting.PrintableComponentLink printableComponentLink1 = new DevExpress.XtraPrinting.PrintableComponentLink(this.components);

            printingSystem1.Links.AddRange(new object[] {
            printableComponentLink1});


            printableComponentLink1.Component = dgv3;

            printableComponentLink1.PrintingSystemBase = printingSystem1;

            printableComponentLink1.EnablePageDialog = true;

            printableComponentLink1.CreateDetailHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateDetailHeaderArea);
            printableComponentLink1.CreateMarginalHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateMarginalHeaderArea);
            printableComponentLink1.BeforeCreateAreas += new System.EventHandler(this.printableComponentLink1_BeforeCreateAreas);
            printableComponentLink1.AfterCreateAreas += new System.EventHandler(this.printableComponentLink1_AfterCreateAreas);
            printableComponentLink1.Landscape = true;

            Printer.setupPrinterMargins(50, 100, 80, 50);

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
            isPrinting = false;
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

            Printer.SetQuadSize(12, 12);
            Printer.DrawQuadBorder(1, 1, 12, 12, BorderSide.All, 1, Color.Black);
            Printer.DrawQuadBorder(12, 1, 1, 12, BorderSide.Right, 1, Color.Black);
        }
        /****************************************************************************************/
        private void printToolStripMenuItem_Click(object sender, EventArgs e)
        {
            isPrinting = true;
            if (this.components == null)
                this.components = new System.ComponentModel.Container();

            DevExpress.XtraPrinting.PrintingSystem printingSystem1 = new DevExpress.XtraPrinting.PrintingSystem(this.components);
            DevExpress.XtraPrinting.PrintableComponentLink printableComponentLink1 = new DevExpress.XtraPrinting.PrintableComponentLink(this.components);

            printingSystem1.Links.AddRange(new object[] {
            printableComponentLink1});


            printableComponentLink1.Component = dgv3;

            printableComponentLink1.PrintingSystemBase = printingSystem1;

            printableComponentLink1.EnablePageDialog = true;

            printableComponentLink1.CreateDetailHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateDetailHeaderArea);
            printableComponentLink1.CreateMarginalHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateMarginalHeaderArea);
            printableComponentLink1.BeforeCreateAreas += new System.EventHandler(this.printableComponentLink1_BeforeCreateAreas);
            printableComponentLink1.AfterCreateAreas += new System.EventHandler(this.printableComponentLink1_AfterCreateAreas);
            printableComponentLink1.Landscape = true;

            Printer.setupPrinterMargins(50, 100, 80, 50);

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
            isPrinting = false;
        }
        /****************************************************************************************/
    }
}