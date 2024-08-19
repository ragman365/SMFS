using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;

using GeneralLib;
using DevExpress.XtraReports.UI;
using DevExpress.XtraReports.ReportGeneration;
using DevExpress.XtraPrinting;
using DevExpress.Utils;
/****************************************************************************************/
namespace SMFS
{
    /****************************************************************************************/
    public partial class AgentSplits : DevExpress.XtraEditors.XtraForm
    {
        /****************************************************************************************/
        private bool modified = false;
        private bool loading = true;
        private string workAgent = "";
        private string workSplits = "";
        private double workPercent = 0D;
        public string agentSplits = "";
        /****************************************************************************************/
        public AgentSplits(string agent, string splits, double percent)
        {
            InitializeComponent();
            workAgent = agent;
            workSplits = splits;
            agentSplits = "";
            workPercent = percent;
        }
        /****************************************************************************************/
        private void AgentSplits_Load(object sender, EventArgs e)
        {
            txtAgent.Text = workAgent;
            LoadData();
        }
        /****************************************************************************************/
        private void LoadData()
        {
            txtTarget.Text = G1.ReformatMoney(workPercent);
            string cmd = "Select * from `agents` order by `agentCode`";
            DataTable dt = G1.get_db_data(cmd);
            dt.Columns.Add("num");
            dt.Columns.Add("extraCommission", Type.GetType("System.Double"));
            dt.Columns.Add("mod");
            G1.NumberDataTable(dt);
            LoadMainAgent(dt);
            dgv.DataSource = dt;
            modified = false;
            loading = false;
        }
        /****************************************************************************************/
        private void LoadMainAgent(DataTable dt)
        {
            if (String.IsNullOrWhiteSpace(workAgent))
                return;
            string[] Lines = workSplits.Split('~');
            string agent = "";
            int row = 0;
            double percent = 0D;
            int increment = 2;
            string str = "";
            for (int i = 0; i < Lines.Length; i = i + increment)
            {
                agent = Lines[i].Trim();
                if (String.IsNullOrWhiteSpace(agent))
                    continue;
                row = LocateAgent(dt, agent);
                if (row >= 0)
                {
                    str = Lines[i + 1].Trim();
                    if (G1.validate_numeric(str))
                    {
                        percent = str.ObjToDouble();
                        dt.Rows[row]["extraCommission"] = percent;
                    }
                }
            }
            CalcNewTotal(dt);
        }
        /***********************************************************************************************/
        private int LocateAgent(DataTable dt, string agent)
        {
            int row = -1;
            string str = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                str = dt.Rows[i]["agentCode"].ObjToString();
                if (str == agent)
                {
                    row = i;
                    break;
                }
            }
            return row;
        }
        /****************************************************************************************/
        private void btnSave_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            DataTable dt = (DataTable)dgv.DataSource;
            string record = "";
            double commission = 0D;
            string agentCode = "";
            string list = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                record = dt.Rows[i]["record"].ObjToString();
                if (String.IsNullOrWhiteSpace(record))
                    continue;
                agentCode = dt.Rows[i]["agentCode"].ObjToString();
                commission = dt.Rows[i]["extraCommission"].ObjToDouble();
                commission = G1.RoundValue(commission);
                if (commission > 0D)
                    list += agentCode + "~" + commission.ToString() + "~";
            }
            agentSplits = list;
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void gridMain_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            if (loading)
                return;
            DataTable dt = (DataTable)dgv.DataSource;
            DataRow dr = gridMain.GetFocusedDataRow();
            string editColumn = e.Column.FieldName.Trim();
            if (editColumn == "extraCommission")
                CalcNewTotal(dt);
            modified = true;
            dr["mod"] = "Y";
        }
        /****************************************************************************************/
        private void CalcNewTotal ( DataTable dt )
        {
            double percent = 0D;
            string str = "";
            double value = 0D;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                str = dt.Rows[i]["extraCommission"].ObjToString();
                if (G1.validate_numeric(str))
                {
                    value = str.ObjToDouble();
                    percent += value;
                }
            }
            txtTotal.Text = G1.ReformatMoney(percent);
        }
        /****************************************************************************************/
        private void AgentSplits_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!modified)
                return;
            DialogResult result = MessageBox.Show("***Question***\nInformation has been modified!\nWould you like to save your changes?", "Add/Edit Agent Splits Dialog", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (result == DialogResult.Cancel)
            {
                e.Cancel = true;
                return;
            }
            modified = false;
            if (result == DialogResult.Yes)
            {
                btnSave_Click(null, null);
                this.DialogResult = DialogResult.OK;
            }
            else if (result == DialogResult.No)
                this.DialogResult = DialogResult.Abort;
        }
        /****************************************************************************************/
    }
}