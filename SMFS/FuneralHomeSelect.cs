using System;
using System.Data;
using System.Windows.Forms;

using GeneralLib;
/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public partial class FuneralHomeSelect : Form
    {
        private DataTable workDt = null;
        /***********************************************************************************************/
        public FuneralHomeSelect( DataTable dt = null )
        {
            InitializeComponent();
            workDt = dt;
        }
        /***********************************************************************************************/
        private void FuneralHomeSelect_Load(object sender, EventArgs e)
        {
            checkPreferences();
            LoadData();
        }
        /***********************************************************************************************/
        private void checkPreferences()
        {
            //string preference = G1.getPreference(LoginForm.username, "Funeral Homes", "Allow Add");
        }
        /***********************************************************************************************/
        private void LoadData ()
        {
            if (workDt == null)
            {
                DataRow dRow = null;
                string cmd = "Select * from `funeralhomes` order by `keycode`;";
                workDt = G1.get_db_data(cmd);
                cmd = "Select * from `cemeteries`;";
                DataTable dx = G1.get_db_data(cmd);
                string loc = "";
                string desc = "";
                for ( int i=0; i<dx.Rows.Count; i++)
                {
                    loc = dx.Rows[i]["loc"].ObjToString();
                    desc = dx.Rows[i]["description"].ObjToString();
                    dRow = workDt.NewRow();
                    dRow["atneedcode"] = loc;
                    dRow["keycode"] = loc;
                    dRow["name"] = desc;
                    dRow["LocationCode"] = desc;
                    dRow["cashLocal"] = dx.Rows[i]["cashLocal"].ObjToString();
                    dRow["checkLocal"] = dx.Rows[i]["checkLocal"].ObjToString();
                    workDt.Rows.Add(dRow);
                }
            }

            workDt.Columns.Add("num");
            G1.NumberDataTable(workDt);
            this.dgv.DataSource = workDt;
        }
        ///***********************************************************************************************/
        //private void CleanupFuneralHomeList( DataTable dt )
        //{
        //    if (G1.get_column_number(dt, "BAD") < 0)
        //        dt.Columns.Add("BAD");

        //    string workAgent = LoginForm.activeFuneralHomeAgent;
        //    string assignedAgents = "";
        //    for (int i = 0; i < dt.Rows.Count; i++)
        //    {
        //        assignedAgents = workDt.Rows[i]["assignedAgents"].ObjToString();
        //        string[] Lines = assignedAgents.Split('~');

        //        DataRow[] agents = dt.Select(workAgent + " IN '" + assignedAgents + "'");
        //        if (agents.Length <= 0)
        //            workDt.Rows[i]["BAD"] = "Y";
        //    }
        //    for (int i = 0; i < workDt.Rows.Count - 1; i--)
        //    {
        //        if (workDt.Rows[i]["BAD"].ObjToString() == "Y")
        //            workDt.Rows.RemoveAt(i);
        //    }
        //}
        /***********************************************************************************************/
        private void btnAddFuneral_Click(object sender, EventArgs e)
        {
            AddHomeNew addhomeFormNew = new AddHomeNew();
            addhomeFormNew.ShowDialog();
            LoadData();
        }
        /***********************************************************************************************/
        private void gridMain_DoubleClick(object sender, EventArgs e)
        {
            int row = gridMain.FocusedRowHandle;
            DataRow dr = gridMain.GetFocusedDataRow();
            string record = dr["record"].ObjToString();
            string keycode = dr["keycode"].ObjToString();
            if ( !String.IsNullOrWhiteSpace ( keycode))
            {
                LoginForm.activeFuneralHomeKeyCode = keycode;
                this.Close();
            }
        }
        /***********************************************************************************************/
    }
}
