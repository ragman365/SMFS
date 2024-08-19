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
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
/****************************************************************************************/
namespace SMFS
{
    /****************************************************************************************/
    public partial class EditAgentLocationEmails: DevExpress.XtraEditors.XtraForm
    {
        private bool modified = false;
        private bool loading = true;
        private string workLocation = "";
        private DataTable agentDt = null;
        private bool editing = false;
        /****************************************************************************************/
        public EditAgentLocationEmails( )
        {
            InitializeComponent();
        }
        /****************************************************************************************/
        private void EditAgentLocationEmails_Load(object sender, EventArgs e)
        {
            this.TopMost = true;
            panelDetailAll.Hide();

            dgv.Dock = DockStyle.Fill;

            string cmd = "Select * from `funeralhomes` f LEFT JOIN `funeral_prospect_locations` g on f.`LocationCode` = g.`LocationCode` ORDER BY f.`LocationCode`;";
            DataTable dt = G1.get_db_data(cmd);
            dt.Columns.Add("num");

            G1.NumberDataTable(dt);
            dgv.DataSource = dt;

            cmd = "Select DISTINCT lastName,firstName from `agents` ORDER BY `lastName`;";
            DataTable agentDt = G1.get_db_data(cmd);
            agentDt.Columns.Add("num");
            agentDt.Columns.Add("email");
            G1.NumberDataTable(agentDt );
            dgv2.DataSource = agentDt;

            G1.SetupToolTip(pictureDelete, "Exit without Saving");
            G1.SetupToolTip(btnAbort, "Exit without Saving");
            G1.SetupToolTip(btnAccept, "Exit and Save Email Data");

            loading = false;
        }
        /****************************************************************************************/
        private void gridMain_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
        }
        /****************************************************************************************/
        private void gridMain_CustomRowFilter(object sender, DevExpress.XtraGrid.Views.Base.RowFilterEventArgs e)
        {
            int row = e.ListSourceRow;
            DataTable dt = (DataTable)dgv.DataSource;
            if (dt == null)
                return;
        }
        /****************************************************************************************/
        private void pictureBox1_Click(object sender, EventArgs e)
        { // Spy Glass
            G1.SpyGlass(gridMain);
        }
        /****************************************************************************************/
        private void gridMain_CustomDrawCell(object sender, DevExpress.XtraGrid.Views.Base.RowCellCustomDrawEventArgs e)
        {
            if (e.Column.FieldName.ToUpper() == "NUM")
            {
                if (e.RowHandle >= 0)
                {
                    string num = (e.RowHandle + 1).ToString();
                    e.DisplayText = num;
                }
            }
        }
        /****************************************************************************************/
        private void gridMain_CustomColumnDisplayText(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventArgs e)
        {
        }
        /****************************************************************************************/
        private int focusedRow = -1;
        private void gridMain_DoubleClick(object sender, EventArgs e)
        {
            int rowHandle = gridMain.FocusedRowHandle;
            gridMain.SelectRow(rowHandle);
            focusedRow = rowHandle;

            DataRow[] dRows = null;

            DataRow dr = gridMain.GetFocusedDataRow();
            workLocation = dr["LocationCode"].ObjToString();
            labDetail.Text = "Working on Locaion " + workLocation;
            string agents = dr["agents"].ObjToString();
            string emails = dr["emails"].ObjToString();

            DataTable dx = (DataTable)dgv2.DataSource;
            for (int i = 0; i < dx.Rows.Count; i++)
                dx.Rows[i]["email"] = "";

            try
            {
                if (!String.IsNullOrWhiteSpace(emails))
                {
                    string[] Lines = emails.Split('~');
                    string[] Lines2 = agents.Split('~');
                    string[] Lines3 = null;
                    string firstName = "";
                    string lastName = "";
                    for (int i = 0; i < Lines2.Length; i++)
                    {
                        Lines3 = Lines2[i].Split(';');
                        if (Lines3.Length < 2)
                            continue;
                        lastName = Lines3[0].Trim();
                        firstName = Lines3[1].Trim();

                        dRows = dx.Select("lastName='" + lastName + "' AND firstName='" + firstName + "'");
                        if (dRows.Length > 0)
                            dRows[0]["email"] = Lines[i].ObjToString();
                    }
                }
            }
            catch ( Exception ex )
            {
            }

            dgv2.DataSource = dx;

            gridMain.OptionsBehavior.Editable = false;
            gridMain.OptionsBehavior.ReadOnly = true;
            gridMain.SelectRow(focusedRow);

            Rectangle rect = dgv.Bounds;
            int left = rect.Left + 150;
            int top = rect.Top + 50;
            int width = rect.Width - 200;
            int height = rect.Height - 75;

            //dgv2.SetBounds(left, top, width, height);
            panelDetailAll.SetBounds (left, top, width, height);
            panelDetailAll.Show();

            //dgv2.Show();
            gridMain2.FocusedRowHandle = 0;
            gridMain2.SelectRow(0);

            editing = true;

            gridMain.OptionsSelection.MultiSelect = false;
            gridMain.OptionsBehavior.KeepFocusedRowOnUpdate = false;
            gridMain.OptionsBehavior.ImmediateUpdateRowPosition = false;
        }
        /****************************************************************************************/
        private void EditAgentLocationEmails_FormClosing(object sender, FormClosingEventArgs e)
        {
        }
        /****************************************************************************************/
        private void btnAbort_Click(object sender, EventArgs e)
        {
            panelDetailAll.Hide();
            dgv.Show();

            editing = false;
        }
        /****************************************************************************************/
        private void btnAccept_Click(object sender, EventArgs e)
        {
            bool found = false;
            string agents = "";
            string emails = "";
            string agentFirstName = "";
            string agentLastName = "";
            string email = "";
            DataRow [] dRows = null;

            DataTable dx = (DataTable) dgv2.DataSource;
            for ( int i=0; i<dx.Rows.Count; i++)
            {
                agentFirstName = dx.Rows[i]["firstName"].ObjToString();
                agentLastName = dx.Rows[i]["lastName"].ObjToString();

                email = dx.Rows[i]["email"].ObjToString();
                if ( !String.IsNullOrWhiteSpace ( email ))
                {
                    emails += email + "~";
                    agents += agentLastName + ";" + agentFirstName + "~";
                    found = true;
                }
            }

            emails = emails.TrimEnd('~');
            agents = agents.TrimEnd('~');

            if (!String.IsNullOrWhiteSpace ( workLocation ))
            {
                DataTable dt = (DataTable)dgv.DataSource;
                dRows = dt.Select("LocationCode='" + workLocation + "'");
                if ( dRows.Length > 0 )
                {
                    dRows[0]["agents"] = agents;
                    dRows[0]["emails"] = emails;
                }
            }

            string record = "";
            string cmd = "Select * from `funeral_prospect_locations` WHERE `LocationCode` = '" + workLocation + "';";
            DataTable rx = G1.get_db_data(cmd);
            if (rx.Rows.Count > 0)
            {
                record = rx.Rows[0]["record"].ObjToString();
                if (String.IsNullOrWhiteSpace(emails))
                    G1.delete_db_table("funeral_prospect_locations", "record", record);
                else
                    G1.update_db_table("funeral_prospect_locations", "record", record, new string[] { "LocationCode", workLocation, "agents", agents, "emails", emails });
            }
            else
            {
                if (!String.IsNullOrWhiteSpace(emails))
                {
                    cmd = "Delete from `funeral_prospect_locations` WHERE `agents` = '-1';";
                    G1.get_db_data(cmd);

                    record = G1.create_record("funeral_prospect_locations", "agents", "-1");
                    if (G1.BadRecord("funeral_prospect_locations", record))
                        return;
                    G1.update_db_table("funeral_prospect_locations", "record", record, new string[] { "LocationCode", workLocation, "agents", agents, "emails", emails });
                }
            }

            panelDetailAll.Hide();

            gridMain.OptionsBehavior.Editable = true;
            gridMain.OptionsBehavior.ReadOnly = false;

            if ( found )
            {
                gridMain.RefreshData();
                gridMain.RefreshEditor(true);
            }
            dgv.Show();
            editing = false;
        }
        /****************************************************************************************/
        private void gridMain_MouseDown(object sender, MouseEventArgs e)
        {
            if ( panelDetailAll.Visible && editing )
            {
                GridView view = sender as GridView;
                //calculate a GridView visual element that is located at the mouse cursor  
                GridHitInfo hi = view.CalcHitInfo(e.X, e.Y);
                if (hi.InRow)
                {
                    //your code  
                    //deny the default MouseDown event handling  
                    (e as DevExpress.Utils.DXMouseEventArgs).Handled = true;

                    int focuedRowHandle = focusedRow;
                    gridMain.RefreshData();
                    gridMain.FocusedRowHandle = focuedRowHandle;
                    gridMain.SelectRow(focuedRowHandle);

                }
            }
        }
        /****************************************************************************************/
        private void gridMain_KeyDown(object sender, KeyEventArgs e)
        {
            if (panelDetailAll.Visible && editing)
            {
                e.Handled = true;
                int focuedRowHandle = focusedRow;
                gridMain.RefreshData();
                gridMain.FocusedRowHandle = focuedRowHandle;
                gridMain.SelectRow(focuedRowHandle);
            }
        }
        /****************************************************************************************/
        private void pictureDelete_Click(object sender, EventArgs e)
        {
            panelDetailAll.Hide();
            dgv.Show();

            editing = false;
        }
        /****************************************************************************************/
    }
}