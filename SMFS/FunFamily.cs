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
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraReports.Design;
using DevExpress.XtraEditors.Controls;
using DevExpress.Utils;
using DevExpress.XtraBars;
using System.Text.RegularExpressions;
using DevExpress.XtraGrid.Views.Grid;
/****************************************************************************************/
namespace SMFS
{
    /****************************************************************************************/
    public partial class FunFamily : DevExpress.XtraEditors.XtraForm
    {
        bool loading = false;
        private string workContract = "";
        private bool funModified = false;
        private bool otherModified = false;
        private bool workLegal = false;
        private DataTable masterDt = null;
        private string workWhat = "";
        private string workFilter = "";
        private bool workFuneral = false;
        private DataTable workDt6 = null;
        /****************************************************************************************/
        public FunFamily(string contract, bool funeral)
        {
            InitializeComponent();
            workContract = contract;
            workLegal = false;
            workFuneral = funeral;
            //workWhat = "ALL";
        }
        /****************************************************************************************/
        public FunFamily(string contract, bool funeral, bool legal = false)
        {
            InitializeComponent();
            workContract = contract;
            workLegal = legal;
            workFuneral = funeral;
        }
        /****************************************************************************************/
        public FunFamily(string contract, string what, bool funeral)
        {
            InitializeComponent();
            workContract = contract;
            workWhat = what;
            workFuneral = funeral;
        }
        /****************************************************************************************/
        public FunFamily(string contract, bool funeral, bool legal = false, string what = "", string filter = "")
        {
            InitializeComponent();
            workContract = contract;
            workLegal = legal;
            workFuneral = funeral;
            workWhat = what;
            workFilter = filter;
        }
        /****************************************************************************************/
        private void FunFamily_Load(object sender, EventArgs e)
        {
            btnSaveAll.Hide();
            SetupMainGrid();
            funModified = false;
            otherModified = false;
            LoadFamily();
            G1.SetupToolTip(pictureBox12, "Add New Member");
            G1.SetupToolTip(pictureBox11, "Remove Member");
            G1.SetupToolTip(picRowUp, "Move Current Member Up 1 Row");
            G1.SetupToolTip(picRowDown, "Move Current Member Down 1 Row");
            addSignatureToolStripMenuItem.Enabled = false;
            //contextMenuStrip1.Enabled = false;
        }
        /****************************************************************************************/
        private void RemoveTabPage(string tabName)
        {
            for (int i = (tabControl1.TabPages.Count - 1); i >= 0; i--)
            {
                TabPage tp = tabControl1.TabPages[i];
                if (tp.Text.ToUpper() == tabName.ToUpper())
                    tabControl1.TabPages.RemoveAt(i);
            }
        }
        /***********************************************************************************************/
        private void SetupMainGrid()
        {
            if (workWhat == "ALL")
            {
                panelFamilyTop.BackColor = Color.MintCream;
                //                this.gridMainDep.PaintStyleName = "Flat";
                this.gridMainLegal.Appearance.BandPanel.BackColor = Color.Yellow;
                this.gridMainLegal.Appearance.HeaderPanel.BackColor = Color.MintCream;
                this.gridMainLegal.Appearance.Empty.BackColor = Color.MintCream;
                this.gridMainLegal.Appearance.EvenRow.BackColor = Color.MintCream;
                this.gridMainLegal.Appearance.EvenRow.BackColor2 = System.Drawing.Color.GhostWhite;

                chkLegal.Hide();
                lblSelect.Hide();
                gridMainDep.Columns["nextOfKin"].Visible = false;
                gridMainDep.Columns["informant"].Visible = false;
                gridMainDep.Columns["purchaser"].Visible = false;
                gridMainDep.Columns["authEmbalming"].Visible = false;
                gridMainDep.Columns["sigs"].Visible = false;
                gridMainDep.Columns["signatureDate"].Visible = false;

                //pictureBox11.Hide();
                //pictureBox12.Hide();
                gridMainLegal.Columns["maidenName"].Visible = false;
                gridMainLegal.Columns["depSuffix"].Visible = false;
                gridMainLegal.Columns["depDOB"].Visible = false;
                gridMainLegal.Columns["depDOD"].Visible = false;
                gridMainLegal.Columns["phone"].Visible = false;
                gridMainLegal.Columns["address"].Visible = false;
                gridMainLegal.Columns["city"].Visible = false;
                gridMainLegal.Columns["state"].Visible = false;
                gridMainLegal.Columns["zip"].Visible = false;
                gridMainLegal.Columns["county"].Visible = false;
                gridMainLegal.Columns["deceased"].Visible = false;
                gridMainLegal.Columns["spouseFirstName"].Visible = false;


                string cmd = "Select * from `relatives` where `contractNumber` = '" + workContract + "' GROUP BY `depRelationship`;";
                DataTable dt = G1.get_db_data(cmd);
                dt.Columns.Add("add");
                dt.Columns.Add("edit");

                chkLegal.Properties.DataSource = dt;
                return;
            }
            if (!workLegal)
            {
                contextMenuStrip1.Enabled = false;
                RemoveTabPage("Disclosures");
                RemoveTabPage("Musicians");
                RemoveTabPage("PallBearers");
                RemoveTabPage("Honorary PallBearers");
                RemoveTabPage("Clergy");
                RemoveTabPage("Other Funeral Information");
                lblFamily.Text = "All Family Members :";

                panelFamilyTop.BackColor = Color.MintCream;
                //                this.gridMainDep.PaintStyleName = "Flat";
                this.gridMainLegal.Appearance.BandPanel.BackColor = Color.Yellow;
                this.gridMainLegal.Appearance.HeaderPanel.BackColor = Color.MintCream;
                this.gridMainLegal.Appearance.Empty.BackColor = Color.MintCream;
                this.gridMainLegal.Appearance.EvenRow.BackColor = Color.MintCream;
                this.gridMainLegal.Appearance.EvenRow.BackColor2 = System.Drawing.Color.GhostWhite;

                chkLegal.Hide();
                lblSelect.Hide();
                gridMainDep.Columns["nextOfKin"].Visible = false;
                gridMainDep.Columns["informant"].Visible = false;
                gridMainDep.Columns["purchaser"].Visible = false;
                gridMainDep.Columns["authEmbalming"].Visible = false;
                gridMainDep.Columns["sigs"].Visible = false;
                gridMainDep.Columns["signatureDate"].Visible = false;
                gridMainDep.Columns["phone"].Visible = true;
                gridMainDep.Columns["phoneType"].Visible = true;
                gridMainDep.Columns["email"].Visible = true;

                //pictureBox11.Hide();
                //pictureBox12.Hide();
                gridMainLegal.Columns["maidenName"].Visible = false;
                gridMainLegal.Columns["depSuffix"].Visible = false;
                gridMainLegal.Columns["depDOB"].Visible = false;
                gridMainLegal.Columns["depDOD"].Visible = false;
                gridMainLegal.Columns["phone"].Visible = false;
                gridMainLegal.Columns["phoneType"].Visible = false;
                gridMainLegal.Columns["email"].Visible = false;
                gridMainLegal.Columns["address"].Visible = false;
                gridMainLegal.Columns["city"].Visible = false;
                gridMainLegal.Columns["state"].Visible = false;
                gridMainLegal.Columns["zip"].Visible = false;
                gridMainLegal.Columns["spouseFirstName"].Visible = false;


                string cmd = "Select * from `relatives` where `contractNumber` = '" + workContract + "' GROUP BY `depRelationship`;";
                DataTable dt = G1.get_db_data(cmd);
                dt.Columns.Add("add");
                dt.Columns.Add("edit");

                chkLegal.Properties.DataSource = dt;

                //pictureBox11.Hide();
                //pictureBox12.Hide();
                //gridMainDep.Columns["maidenName"].Visible = false;
                //gridMainDep.Columns["depSuffix"].Visible = false;
                //gridMainDep.Columns["depDOB"].Visible = false;
                //gridMainDep.Columns["depDOD"].Visible = false;
                //gridMainDep.Columns["phone"].Visible = false;
                //gridMainDep.Columns["address"].Visible = false;
                //gridMainDep.Columns["city"].Visible = false;
                //gridMainDep.Columns["state"].Visible = false;
                //gridMainDep.Columns["zip"].Visible = false;
                //gridMainDep.Columns["spouseFirstName"].Visible = false;
            }
            else
            {
                RemoveTabPage("Family Members");
                RemoveTabPage("Legal Members");
            }
            if (!String.IsNullOrWhiteSpace(workWhat))
            {
                if (workWhat.ToUpper() == "ALL")
                    return;
                if (workWhat.ToUpper() != "FAMILY MEMBERS")
                    RemoveTabPage("Family Members");
                if (workWhat.ToUpper() != "LEGAL MEMBERS")
                    RemoveTabPage("Legal Members");
                if (workWhat.ToUpper() != "MUSICIANS")
                    RemoveTabPage("Musicians");
                if (workWhat.ToUpper() != "PALLBEARERS")
                    RemoveTabPage("PallBearers");
                if (workWhat.ToUpper() != "HONORARY PALLBEARERS")
                    RemoveTabPage("Honorary PallBearers");
                if (workWhat.ToUpper() != "CLERGY")
                    RemoveTabPage("Clergy");
                if (workWhat.ToUpper() != "OTHER FUNERAL INFORMATION")
                    RemoveTabPage("Other Funeral Information");
                if (workWhat.ToUpper() == "OTHER FUNERAL INFORMATION")
                {
                    if (!String.IsNullOrWhiteSpace(workFilter))
                    {

                    }
                }
            }
        }
        /***********************************************************************************************/
        private void LoadFamily()
        {
            LoadDBTable("ref_relations", "relationship", this.repositoryItemComboBox2);
            LoadDBTable("ref_states", "abbrev", this.repositoryItemComboBox1);
            funModified = false;
            if (String.IsNullOrWhiteSpace(workContract))
                return;
            string cmd = "Select * from `relatives` where `contractNumber` = '" + workContract + "';";
            DataTable dt = G1.get_db_data(cmd);
            dt.Columns.Add("num");
            dt.Columns.Add("mod");
            dt.Columns.Add("sigs", typeof(Bitmap));
            dt.Columns.Add("add");
            dt.Columns.Add("edit");

            addSignatureToolStripMenuItem.Enabled = true;
            contextMenuStrip1.Enabled = true;

            if (workWhat == "ALL")
            {
                LoadFamilyMembers(dt);
                LoadPallBearers(dt);
                LoadHonoraryPallBearers(dt);
                LoadClergy(dt);
                LoadMusicians(dt);
                LoadDisclosures(dt);
                LoadOtherData();
            }
            else
            {
                if (!workLegal)
                    LoadFamilyMembers(dt);
                else
                {
                    dgvDependent.Visible = false;
                    dgvLegal.Visible = false;
                    LoadPallBearers(dt);
                    LoadHonoraryPallBearers(dt);
                    LoadClergy(dt);
                    LoadMusicians(dt);
                    LoadDisclosures(dt);
                    LoadOtherData();
                }
            }
            masterDt = dt;
        }
        /***************************************************************************************/
        private void LoadFamilyMembers(DataTable dt)
        {
            DataTable famDt = dt.Clone();
            string relation = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                relation = dt.Rows[i]["depRelationship"].ObjToString().ToUpper();
                if (relation == "PB")
                    continue;
                if (relation == "HPB")
                    continue;
                if (relation == "CLERGY")
                    continue;
                if (relation == "MUSICIAN")
                    continue;
                if (relation == "DISCLOSURES")
                    continue;
                G1.copy_dt_row(dt, i, famDt, famDt.Rows.Count);
                int row = famDt.Rows.Count - 1;
                famDt.Rows[row]["signature"] = dt.Rows[i]["signature"].ObjToBytes();
            }
            G1.sortTable(famDt, "MemOrder", "ASC");
            SetupNextOfKin(famDt);
            SetupPB(famDt, dt);
            SetupHPB(famDt, dt);
            LoadSignatures(famDt);
            G1.NumberDataTable(famDt);
            dgvDependent.DataSource = famDt;
            //            dgvLegal.DataSource = famDt;
        }
        /***************************************************************************************/
        private DateTime lastPallBearersTime = DateTime.Now;
        private void LoadPallBearers(DataTable dt)
        {
            DataTable pallDt = dt.Clone();
            string relation = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                relation = dt.Rows[i]["depRelationship"].ObjToString().ToUpper();
                if (relation != "PB")
                    continue;
                G1.copy_dt_row(dt, i, pallDt, pallDt.Rows.Count);
            }

            G1.sortTable(pallDt, "PalOrder", "ASC");

            G1.NumberDataTable(pallDt);
            dgv2.DataSource = pallDt;
            lastPallBearersTime = DateTime.Now;
        }
        /***************************************************************************************/
        private void LoadHonoraryPallBearers(DataTable dt)
        {
            DataTable pallDt = dt.Clone();
            string relation = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                relation = dt.Rows[i]["depRelationship"].ObjToString().ToUpper();
                if (relation != "HPB")
                    continue;
                G1.copy_dt_row(dt, i, pallDt, pallDt.Rows.Count);
            }

            G1.sortTable(pallDt, "Order", "ASC");

            G1.NumberDataTable(pallDt);
            dgv3.DataSource = pallDt;
        }
        /***************************************************************************************/
        private void LoadClergy(DataTable dt)
        {
            DataTable pallDt = dt.Clone();
            string relation = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                relation = dt.Rows[i]["depRelationship"].ObjToString().ToUpper();
                if (relation != "CLERGY")
                    continue;
                G1.copy_dt_row(dt, i, pallDt, pallDt.Rows.Count);
            }

            G1.sortTable(pallDt, "Order", "ASC");

            string fullName = "";
            for (int i = 0; i < pallDt.Rows.Count; i++)
            {
                fullName = pallDt.Rows[i]["fullName"].ObjToString();
                if (String.IsNullOrWhiteSpace(fullName))
                {
                    fullName = BuildClergyName(pallDt, i);
                    pallDt.Rows[i]["fullName"] = fullName;
                }
            }
            G1.NumberDataTable(pallDt);
            dgv4.DataSource = pallDt;
            gridMain4.Columns["depFirstName"].Visible = false;
            gridMain4.Columns["depLastName"].Visible = false;
            gridMain4.Columns["depMI"].Visible = false;
            gridMain4.Columns["depPrefix"].Visible = true;
            gridMain4.Columns["depSuffix"].Visible = true;
        }
        /***************************************************************************************/
        private void LoadMusicians(DataTable dt)
        {
            DataTable musicianDt = dt.Clone();
            string relation = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                relation = dt.Rows[i]["depRelationship"].ObjToString().ToUpper();
                if (relation != "MUSICIAN")
                    continue;
                G1.copy_dt_row(dt, i, musicianDt, musicianDt.Rows.Count);
            }

            G1.sortTable(musicianDt, "Order", "ASC");

            G1.NumberDataTable(musicianDt);
            dgv5.DataSource = musicianDt;
        }
        /***************************************************************************************/
        private void LoadDisclosures(DataTable dt)
        {
            DataTable disclosureDt = dt.Clone();
            string relation = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                relation = dt.Rows[i]["depRelationship"].ObjToString().ToUpper();
                if (relation != "DISCLOSURES")
                    continue;
                G1.copy_dt_row(dt, i, disclosureDt, disclosureDt.Rows.Count);
            }

            if (disclosureDt.Rows.Count <= 0)
            {
                string disclosure = "";
                string answer = "";
                DataTable dx = G1.get_db_data("Select * from `disclosures`;");
                if (dx.Rows.Count > 0)
                {
                    for (int i = 0; i < dx.Rows.Count; i++)
                    {
                        disclosure = dx.Rows[i]["disclosure"].ObjToString();
                        answer = dx.Rows[i]["answer"].ObjToString();
                        AddDisclosureRow(disclosureDt, disclosure, answer);
                    }
                }
            }

            G1.NumberDataTable(disclosureDt);
            dgv7.DataSource = disclosureDt;
        }
        /***************************************************************************************/
        private void AddDisclosureRow(DataTable disclosureDt, string what, string answer)
        {
            DataRow dR = disclosureDt.NewRow();
            dR["contractNumber"] = workContract;
            dR["depFirstName"] = what;
            dR["depLastName"] = answer;
            dR["nextOfKin"] = "0";
            dR["depRelationship"] = "DISCLOSURE";
            disclosureDt.Rows.Add(dR);
        }
        /***************************************************************************************/
        public bool FireEventFunServicesModified()
        {
            if (funModified)
                return true;
            return false;
        }
        /***************************************************************************************/
        public void FireEventFunServicesSetModified()
        {
            funModified = true;
            this.btnSaveAll.Show();
            this.btnSaveAll.Refresh();
        }
        /***************************************************************************************/
        public void FireEventSaveFunServices(bool save = false, bool stayOpen = false)
        {
            if (save && funModified)
            {
                SaveRelatives();
                if (otherModified)
                {
                    DataTable dt = (DataTable)dgv6.DataSource;
                    SaveOtherData(workContract, dt, workFuneral);
                }
                funModified = false;
                btnSaveAll.Hide();
            }
            if (!stayOpen)
                this.Close();
        }
        /***********************************************************************************************/
        private void SaveRelatives()
        {
            DataTable dt = null;
            if (!workLegal || workWhat == "ALL")
            {
                dt = (DataTable)dgvDependent.DataSource;
                SaveMembers(dt);
                if (workWhat != "ALL")
                    return;
            }

            dt = (DataTable)dgv2.DataSource;
            SaveMembers(dt, "PB");

            dt = (DataTable)dgv3.DataSource;
            SaveMembers(dt, "HPB");

            dt = (DataTable)dgv4.DataSource;
            SaveMembers(dt, "CLERGY");

            dt = (DataTable)dgv5.DataSource;
            SaveMembers(dt, "MUSICIAN");

            dt = (DataTable)dgv7.DataSource;
            SaveMembers(dt, "DISCLOSURES");
        }
        /***********************************************************************************************/
        private void SaveMembers(DataTable dt, string addRelation = "")
        {
            string record = "";
            string mod = "";
            string prefix = "";
            string firstName = "";
            string lastName = "";
            string mi = "";
            string suffix = "";
            string fullName = "";
            string dob = "";
            string dod = "";
            string relationship = "";
            string maidenName = "";
            string address = "";
            string city = "";
            string state = "";
            string zip = "";
            string spouseFirstName = "";
            string phone = "";
            string phoneType = "";
            string email = "";
            DateTime date = DateTime.Now;

            string nextOfKin = "";
            string informant = "";
            string purchaser = "";
            string authEmbalming = "";
            string deceased = "";
            string legOrder = "";

            int order = 0;
            string fieldOrder = "Order";
            if (String.IsNullOrEmpty(addRelation))
                fieldOrder = "MemOrder";
            if (addRelation.ToUpper() == "PB")
                fieldOrder = "PalOrder";

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                record = dt.Rows[i]["record"].ObjToString();
                if (record == "-1")
                    record = "";
                mod = dt.Rows[i]["mod"].ObjToString();
                if (mod == "D")
                {
                    if (!String.IsNullOrWhiteSpace(record))
                        G1.delete_db_table("relatives", "record", record);
                    continue;
                }
                if (String.IsNullOrWhiteSpace(record))
                    record = G1.create_record("relatives", "depFirstName", "-1");
                if (G1.BadRecord("relatives", record))
                    return;
                firstName = dt.Rows[i]["depFirstName"].ObjToString();
                lastName = dt.Rows[i]["depLastName"].ObjToString();
                mi = dt.Rows[i]["depMI"].ObjToString();
                suffix = dt.Rows[i]["depSuffix"].ObjToString();
                prefix = dt.Rows[i]["depPrefix"].ObjToString();
                dob = dt.Rows[i]["depDOB"].ObjToString();
                dod = dt.Rows[i]["depDOD"].ObjToString();
                maidenName = dt.Rows[i]["maidenName"].ObjToString();
                relationship = dt.Rows[i]["depRelationship"].ObjToString();
                if (!String.IsNullOrWhiteSpace(addRelation))
                    relationship = addRelation;
                fullName = "";
                if (relationship.ToUpper() == "CLERGY")
                {
                    fullName = dt.Rows[i]["fullName"].ObjToString();
                    if (!String.IsNullOrWhiteSpace(fullName))
                        G1.ParseOutName(fullName, ref prefix, ref firstName, ref lastName, ref mi, ref suffix);
                }
                address = dt.Rows[i]["address"].ObjToString();
                city = dt.Rows[i]["city"].ObjToString();
                state = dt.Rows[i]["state"].ObjToString();
                zip = dt.Rows[i]["zip"].ObjToString();
                spouseFirstName = dt.Rows[i]["spouseFirstName"].ObjToString();
                phone = dt.Rows[i]["phone"].ObjToString();
                phoneType = dt.Rows[i]["phoneType"].ObjToString();
                email = dt.Rows[i]["email"].ObjToString();
                G1.update_db_table("relatives", "record", record, new string[] { "depFirstName", firstName, "depLastName", lastName, "contractNumber", workContract, "depMI", mi, "depPrefix", prefix, "depSuffix", suffix, "depDOB", dob, "depRelationship", relationship, "maidenName", maidenName, "fullName", fullName });
                G1.update_db_table("relatives", "record", record, new string[] { "address", address, "city", city, "state", state, "zip", zip, "spouseFirstName", spouseFirstName, "depDOD", dod, "phone", phone, "phoneType", phoneType, "email", email });

                nextOfKin = dt.Rows[i]["nextOfKin"].ObjToString();
                informant = dt.Rows[i]["informant"].ObjToString();
                purchaser = dt.Rows[i]["purchaser"].ObjToString();
                deceased = dt.Rows[i]["deceased"].ObjToString();
                legOrder = dt.Rows[i]["LegOrder"].ObjToString();
                authEmbalming = dt.Rows[i]["authEmbalming"].ObjToString();

                order++;

                G1.update_db_table("relatives", "record", record, new string[] { "LegOrder", legOrder, "nextOfKin", nextOfKin, "informant", informant, "purchaser", purchaser, "deceased", deceased, "authEmbalming", authEmbalming, fieldOrder, order.ToString() });
                dt.Rows[i]["record"] = record;
            }
        }
        /***********************************************************************************************/
        private void SaveLegal()
        {
            string record = "";
            string mod = "";
            string firstName = "";
            string lastName = "";
            string mi = "";
            string suffix = "";
            string dob = "";
            string dod = "";
            string relationship = "";
            string maidenName = "";
            string city = "";
            string spouseFirstName = "";
            string phone = "";
            string nextOfKin = "";
            string informant = "";
            string purchaser = "";
            string authEmbalming = "";
            DataTable dt = (DataTable)dgvDependent.DataSource;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                record = dt.Rows[i]["record"].ObjToString();
                if (String.IsNullOrWhiteSpace(record))
                    continue;
                nextOfKin = dt.Rows[i]["nextOfKin"].ObjToString();
                informant = dt.Rows[i]["informant"].ObjToString();
                purchaser = dt.Rows[i]["purchaser"].ObjToString();
                authEmbalming = dt.Rows[i]["authEmbalming"].ObjToString();

                G1.update_db_table("relatives", "record", record, new string[] { "nextOfKin", nextOfKin, "informant", informant, "purchaser", purchaser, "authEmbalming", authEmbalming });
            }
        }
        /***************************************************************************************/
        private void repositoryItemDateEdit1_CalendarTimeProperties_EditValueChanged(object sender, EventArgs e)
        {

        }
        /****************************************************************************************/
        private void pictureBox12_Click(object sender, EventArgs e)
        {
            int row = AddNewRow();
            int rowHandle = row - 1;
            //DataTable dt = GetCurrentDataTable();
            //int rowHandle = GetCurrentGridView().FocusedRowHandle;
            GetCurrentGridView().FocusedRowHandle = rowHandle;
            GetCurrentGridView().SelectRow(rowHandle);
            if (GetCurrentGridView().VisibleColumns.Count > 0)
            {
                GridColumn firstColumn = GetCurrentGridView().Columns["fullName"];
                if (!firstColumn.Visible)
                {
                    firstColumn = GetCurrentGridView().Columns["depFirstName"];
                    if (!firstColumn.Visible)
                        firstColumn = GetCurrentGridView().VisibleColumns[1];
                }
                GetCurrentGridView().FocusedColumn = GetCurrentGridView().Columns[firstColumn.FieldName];
            }
        }
        /***********************************************************************************************/
        private int AddNewRow()
        {
            DataTable dt = GetCurrentDataTable();
            DataRow dRow = dt.NewRow();
            if (G1.get_column_number(dt, "pb") >= 0)
                dRow["pb"] = "0";
            if (G1.get_column_number(dt, "hpb") >= 0)
                dRow["hpb"] = "0";
            if (G1.get_column_number(dt, "deceased") >= 0)
                dRow["deceased"] = "0";
            dRow["num"] = (dt.Rows.Count + 1).ToString();
            dRow["authEmbalming"] = "0";
            dt.Rows.Add(dRow);
            int row = dt.Rows.Count;
            GetCurrentDataGrid().DataSource = dt;
            GetCurrentDataGrid().Refresh();
            gridMainDep_CellValueChanged(null, null);
            return row;
        }
        ///***********************************************************************************************/
        //private void AddMod(DataTable dt, DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView grid)
        //{
        //    if (G1.get_column_number(dt, "mod") < 0)
        //    {
        //        dt.Columns.Add("mod");
        //    }
        //}
        /****************************************************************************************/
        private void ClergyChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            DataRow dr = gridMain4.GetFocusedDataRow();
            if (dr == null)
                return;
            dr["mod"] = "Y";
            funModified = true;
            btnSaveAll.Show();
            GridColumn currCol = gridMain4.FocusedColumn;
            currentColumn = currCol.FieldName;
            if (gridMain4.Columns[currentColumn].ColumnEdit != null)
            {
                DataTable dt = (DataTable)dgv4.DataSource;
                int rowHandle = gridMain4.FocusedRowHandle;
                int row = gridMain4.GetDataSourceRowIndex(rowHandle);
                //                string what = BuildClergyName(dt, row);
                string what = dr[currentColumn].ObjToString();
                string answers = "";
                bool found = false;
                for (int i = 0; i < ciLookup.Items.Count; i++)
                {
                    answers = ciLookup.Items[i].ObjToString();
                    if (what.Trim().ToUpper() == answers.Trim().ToUpper())
                    {
                        found = true;
                    }
                }
                if (!found)
                {
                    dr["add"] = "+";
                    dr["edit"] = "E";
                }
                else
                {
                    dr["add"] = "";
                    dr["edit"] = "";
                }
            }
        }
        /****************************************************************************************/
        private void gridMainDep_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            if (dgv4.Visible)
            {
                ClergyChanged(sender, e);
                return;
            }
            DataTable dt = GetCurrentDataTable();
            if (dt == null)
                return;
            DataRow dr = GetCurrentGridView().GetFocusedDataRow();
            if (e != null)
            {
                if (dgvDependent != null)
                {
                    if (dgvDependent.Visible)
                    {
                        if (e.Column.FieldName.ToUpper() == "PB")
                            return;
                    }
                }
                if (e.Column.FieldName.ToUpper() == "FINANCIALLYDEPENDENT")
                {
                    string data = dr["financiallyDependent"].ObjToString();
                    if (!String.IsNullOrWhiteSpace(data))
                    {
                        string what = data.Substring(0, 1).ToUpper();
                        if (what == "Y")
                            what = "YES";
                        else if (what == "N")
                            what = "NO";
                        else
                        {
                            MessageBox.Show("***ERROR*** Must be 'Yes' or 'No!");
                            what = "";
                        }
                        dr["financiallyDependent"] = what;
                    }
                }
                else if (e.Column.FieldName.ToUpper() == "ZIP")
                {
                    string zipCode = dr["zip"].ObjToString();
                    if (!String.IsNullOrWhiteSpace(zipCode))
                    {
                        string city = "";
                        string state = "";
                        string county = "";
                        bool rv = LookupZipcode(zipCode, ref city, ref state, ref county);
                        if (rv)
                        {
                            if (!String.IsNullOrWhiteSpace(state))
                            {
                                string cmd = "Select * from `ref_states` where `state` = '" + state + "';";
                                DataTable dx = G1.get_db_data(cmd);
                                if (dx.Rows.Count > 0)
                                    state = dx.Rows[0]["abbrev"].ObjToString();
                            }
                            if (!String.IsNullOrWhiteSpace(city))
                                dr["city"] = city;
                            if (!String.IsNullOrWhiteSpace(state))
                                dr["state"] = state;
                            if (!String.IsNullOrWhiteSpace(county))
                                dr["county"] = county;
                        }
                    }
                }
                else if (e.Column.FieldName.ToUpper() == "DEPDOD")
                {
                    string dod = dr["depDOD"].ObjToString();
                    DateTime deceasedDate = dod.ObjToDateTime();
                    if (deceasedDate.Year > 100)
                    {
                        dr["deceased"] = "1";
                    }
                }
                dr["mod"] = "Y";
            }
            funModified = true;
            btnSaveAll.Show();
            OnFamilyModified();
        }
        /****************************************************************************************/
        public static bool LookupZipcode(string zipCode, ref string city, ref string state, ref string county)
        {
            city = "";
            state = "";
            county = "";
            bool rv = false;
            if (String.IsNullOrWhiteSpace(zipCode))
                return rv;
            if (!G1.validate_numeric(zipCode))
                return rv;
            string cmd = "Select * from `ref_zipcodes` where `zipcode` = '" + zipCode + "';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count > 0)
            {
                rv = true;
                city = dx.Rows[0]["city"].ObjToString();
                state = dx.Rows[0]["state"].ObjToString();
                county = dx.Rows[0]["county"].ObjToString();
            }
            return rv;
        }
        /****************************************************************************************/
        private void gridMainDep_CustomDrawCell(object sender, DevExpress.XtraGrid.Views.Base.RowCellCustomDrawEventArgs e)
        {
            if (e.Column.FieldName.ToUpper() == "NUM")
            {
                if (e.RowHandle >= 0)
                {
                    string num = (e.RowHandle + 1).ToString();
                    e.DisplayText = num;
                }
                return;
            }

            if (e.Column.FieldName.ToUpper() == "DEPDOB" || e.Column.FieldName.ToUpper() == "DEPDOD" ||
                e.Column.FieldName.ToUpper() == "SIGNATUREDATE")
            {
                if (!String.IsNullOrWhiteSpace(e.DisplayText))
                {
                    if (!G1.validate_date(e.DisplayText))
                        e.DisplayText = "";
                    else
                    {
                        DateTime date = e.DisplayText.ObjToDateTime();
                        if (date.Year < 1875)
                            e.DisplayText = "";
                        else
                            e.DisplayText = date.ToString("MM/dd/yyyy");
                    }
                }
            }
        }
        /****************************************************************************************/
        private DevExpress.XtraGrid.GridControl GetCurrentDataGrid()
        {
            DevExpress.XtraGrid.GridControl currentDGV = null;
            if (dgvDependent.Visible)
                currentDGV = dgvDependent;
            else if (dgvLegal.Visible)
                currentDGV = dgvLegal;
            else if (dgv2.Visible)
                currentDGV = dgv2;
            else if (dgv3.Visible)
                currentDGV = dgv3;
            else if (dgv4.Visible)
                currentDGV = dgv4;
            else if (dgv5.Visible)
                currentDGV = dgv5;
            return currentDGV;
        }
        /****************************************************************************************/
        private DataTable GetCurrentDataTable()
        {
            DataTable dt = null;
            if (dgvDependent.Visible)
                dt = (DataTable)dgvDependent.DataSource;
            else if (dgv2.Visible)
                dt = (DataTable)dgv2.DataSource;
            else if (dgv3.Visible)
                dt = (DataTable)dgv3.DataSource;
            else if (dgv4.Visible)
                dt = (DataTable)dgv4.DataSource;
            else if (dgv5.Visible)
                dt = (DataTable)dgv5.DataSource;
            else if (dgv6.Visible)
                dt = (DataTable)dgv6.DataSource;
            else if (dgvLegal.Visible)
                dt = (DataTable)dgvLegal.DataSource;
            return dt;
        }
        /****************************************************************************************/
        private DataRow GetCurrentDataRow()
        {
            DataRow dr = null;
            if (dgvDependent.Visible)
                dr = gridMainDep.GetFocusedDataRow();
            else if (dgvLegal.Visible)
                dr = gridMainLegal.GetFocusedDataRow();
            else if (dgv2.Visible)
                dr = gridMain2.GetFocusedDataRow();
            else if (dgv3.Visible)
                dr = gridMain3.GetFocusedDataRow();
            else if (dgv4.Visible)
                dr = gridMain4.GetFocusedDataRow();
            else if (dgv5.Visible)
                dr = gridMain5.GetFocusedDataRow();
            else if (dgv6.Visible)
                dr = gridMain6.GetFocusedDataRow();
            return dr;
        }
        /****************************************************************************************/
        private DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView GetCurrentGridView()
        {
            DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gv = null;
            if (dgvDependent.Visible)
                gv = gridMainDep;
            else if (dgv2.Visible)
                gv = gridMain2;
            else if (dgv3.Visible)
                gv = gridMain3;
            else if (dgv4.Visible)
                gv = gridMain4;
            else if (dgv5.Visible)
                gv = gridMain5;
            else if (dgv6.Visible)
                gv = gridMain6;
            else if (dgvLegal.Visible)
                gv = gridMainLegal;
            return gv;
        }
        /****************************************************************************************/
        private void pictureBox11_Click(object sender, EventArgs e)
        {
            DataRow dr = GetCurrentDataRow();
            if (dr == null)
                return;
            string relation = dr["depRelationship"].ObjToString().Trim();
            string firstName = dr["depFirstName"].ObjToString();
            string lastName = dr["depLastName"].ObjToString();
            string name = lastName + ", " + firstName;
            string fullName = dr["fullName"].ObjToString();
            if (dgv4.Visible && !String.IsNullOrWhiteSpace(fullName))
                name = fullName;
            DialogResult result = MessageBox.Show("***Question*** Are you sure you want to DELETE this Relation\n(" + name + ") ?", "Delete Relation Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No)
                return;
            DataTable dt = GetCurrentDataTable();
            if (dt == null)
                return;
            int rowHandle = GetCurrentGridView().FocusedRowHandle;
            int row = GetCurrentGridView().GetDataSourceRowIndex(rowHandle);
            if (relation == "HPB" || relation == "PB" || relation == "CLERGY")
            {
                string record = dr["record"].ObjToString();
                if (!String.IsNullOrWhiteSpace(record))
                    G1.delete_db_table("relatives", "record", record);
                dt.Rows.Remove(dr);
                GetCurrentDataGrid().DataSource = dt;
                GetCurrentDataGrid().RefreshDataSource();
            }
            else
            {
                dr["Mod"] = "D";
                dt.Rows[row]["mod"] = "D";
            }
            funModified = true;
            btnSaveAll.Show();

            GetCurrentGridView().RefreshData();
            GetCurrentGridView().RefreshEditor(true);
        }
        /****************************************************************************************/
        private void gridMainDep_CustomRowFilter(object sender, DevExpress.XtraGrid.Views.Base.RowFilterEventArgs e)
        {
            int row = e.ListSourceRow;
            //            DataTable dt = (DataTable)dgvDependent.DataSource;
            DataTable dt = GetCurrentDataTable();
            if (dt == null)
                return;
            try
            {
                string delete = dt.Rows[row]["mod"].ObjToString();
                if (delete.ToUpper() == "D")
                {
                    e.Visible = false;
                    e.Handled = true;
                }
            }
            catch (Exception ex)
            {
                // MessageBox.Show("***ERROR*** Not Showing Deleted Members for Contract " + workContract + " Error " + ex.Message.ToString());
            }
        }
        /****************************************************************************************/
        private void panelAll_Paint(object sender, PaintEventArgs e)
        {
        }
        /****************************************************************************************/
        private void panelFamilyTop_Paint(object sender, PaintEventArgs e)
        {
            Rectangle rect = panelFamilyTop.Bounds;
            Graphics g = panelFamilyTop.CreateGraphics();
            Pen pen = new Pen(Brushes.Black);
            int left = rect.Left;
            int top = rect.Top;
            int width = this.panelAll.Width - 2;
            int high = rect.Height - 2;
            g.DrawRectangle(pen, left, top, width, high);
        }
        /****************************************************************************************/
        private void panelBottom_Paint(object sender, PaintEventArgs e)
        {
            Rectangle rect = panelBottom.Bounds;
            Graphics g = panelBottom.CreateGraphics();
            Pen pen = new Pen(Brushes.Black);
            int left = rect.Left;
            int top = rect.Top;
            int width = rect.Width - 2;
            int high = rect.Height - 2;
            g.DrawRectangle(pen, left, top, width, high);
        }
        /***************************************************************************************/
        public delegate void d_void_eventdone_string(string s);
        public event d_void_eventdone_string FamilyModifiedDone;
        protected void OnFamilyModified()
        {
            if (FamilyModifiedDone != null)
            {
                //                DataRow dr = gridMainDep.GetFocusedDataRow();
                FamilyModifiedDone.Invoke("YES");
            }
        }
        /***********************************************************************************************/
        private void LoadSignatures(DataTable dt)
        {
            if (dt == null)
                return;
            Bitmap emptyImage = new Bitmap(1, 1);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                Byte[] bytes = dt.Rows[i]["signature"].ObjToBytes();
                Image myImage = emptyImage;
                if (bytes != null)
                    myImage = G1.byteArrayToImage(bytes);
                dt.Rows[i]["sigs"] = (Bitmap)myImage;
            }
        }
        /***********************************************************************************************/
        private void SetupPB(DataTable dt, DataTable relationDt)
        {
            DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit selectnew = this.repositoryItemCheckEdit7;
            selectnew.NullText = "";
            selectnew.ValueChecked = "1";
            selectnew.ValueUnchecked = "0";
            selectnew.ValueGrayed = "";
            if (G1.get_column_number(dt, "pb") < 0)
                dt.Columns.Add("pb");
            string fname = "";
            string lname = "";
            string mi = "";
            DataRow[] dR = null;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                fname = dt.Rows[i]["depFirstName"].ObjToString();
                lname = dt.Rows[i]["depLastName"].ObjToString();
                mi = dt.Rows[i]["depMI"].ObjToString();
                dR = relationDt.Select("depFirstName='" + fname + "' AND depLastName='" + lname + "' AND depMI='" + mi + "' AND depRelationship='PB'");
                if (dR.Length > 0)
                    dt.Rows[i]["pb"] = "1";
                else
                    dt.Rows[i]["pb"] = "0";
            }
        }
        /***********************************************************************************************/
        private void SetupHPB(DataTable dt, DataTable relationDt)
        {
            DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit selectnew = this.repositoryItemCheckEdit12;
            selectnew.NullText = "";
            selectnew.ValueChecked = "1";
            selectnew.ValueUnchecked = "0";
            selectnew.ValueGrayed = "";
            if (G1.get_column_number(dt, "hpb") < 0)
                dt.Columns.Add("hpb");
            string fname = "";
            string lname = "";
            string mi = "";
            DataRow[] dR = null;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                fname = dt.Rows[i]["depFirstName"].ObjToString();
                lname = dt.Rows[i]["depLastName"].ObjToString();
                mi = dt.Rows[i]["depMI"].ObjToString();
                dR = relationDt.Select("depFirstName='" + fname + "' AND depLastName='" + lname + "' AND depMI='" + mi + "' AND depRelationship='HPB'");
                if (dR.Length > 0)
                    dt.Rows[i]["hpb"] = "1";
                else
                    dt.Rows[i]["hpb"] = "0";
            }
        }
        /***********************************************************************************************/
        private void SetupNextOfKin(DataTable dt)
        {
            DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit selectnew = this.repositoryItemCheckEdit1;
            selectnew.NullText = "";
            selectnew.ValueChecked = "1";
            selectnew.ValueUnchecked = "0";
            selectnew.ValueGrayed = "";

            DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit selectnew1 = this.repositoryItemCheckEdit6;
            selectnew1.NullText = "";
            selectnew1.ValueChecked = "1";
            selectnew1.ValueUnchecked = "0";
            selectnew1.ValueGrayed = "";
            string text = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                text = dt.Rows[i]["nextOfKin"].ObjToString();
                if (!String.IsNullOrWhiteSpace(text))
                    dt.Rows[i]["nextOfKin"] = text;
                else
                    dt.Rows[i]["nextOfKin"] = "0";

                text = dt.Rows[i]["informant"].ObjToString();
                if (!String.IsNullOrWhiteSpace(text))
                    dt.Rows[i]["informant"] = text;
                else
                    dt.Rows[i]["informant"] = "0";

                text = dt.Rows[i]["purchaser"].ObjToString();
                if (!String.IsNullOrWhiteSpace(text))
                    dt.Rows[i]["purchaser"] = text;
                else
                    dt.Rows[i]["purchaser"] = "0";

                text = dt.Rows[i]["authEmbalming"].ObjToString();
                if (!String.IsNullOrWhiteSpace(text))
                    dt.Rows[i]["authEmbalming"] = text;
                else
                    dt.Rows[i]["authEmbalming"] = "0";

                text = dt.Rows[i]["deceased"].ObjToString();
                if (!String.IsNullOrWhiteSpace(text))
                    dt.Rows[i]["deceased"] = text;
                else
                    dt.Rows[i]["deceased"] = "0";
            }
            DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit selectnew2 = this.repositoryItemCheckEdit8;
            selectnew2.NullText = "";
            selectnew2.ValueChecked = "1";
            selectnew2.ValueUnchecked = "0";
            selectnew2.ValueGrayed = "";
        }
        /****************************************************************************************/
        private void repositoryItemCheckEdit1_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMainLegal.GetFocusedDataRow();
            DataTable dt = (DataTable)dgvLegal.DataSource;
            DataTable dx = (DataTable)dgvDependent.DataSource;
            try
            {
                if (!String.IsNullOrWhiteSpace(currentColumn))
                {
                    if (currentColumn != "deceased")
                    {
                        if (currentColumn != "purchaser")
                        {
                            try
                            {
                                for (int i = 0; i < dt.Rows.Count; i++)
                                    dt.Rows[i][currentColumn] = "0";
                                for (int i = 0; i < dx.Rows.Count; i++)
                                    dx.Rows[i][currentColumn] = "0";
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("***ERROR*** Clearing CheckBoxes for Contract " + workContract + " Error " + ex.Message.ToString());
                            }
                        }
                    }
                }
                int rowHandle = gridMainLegal.FocusedRowHandle;
                int row = gridMainLegal.GetDataSourceRowIndex(rowHandle);
                dr["Mod"] = "Y";
                dt.Rows[row]["mod"] = "Y";
                if (dt.Rows[row][currentColumn].ObjToString() == "1")
                    dt.Rows[row][currentColumn] = "0";
                else
                    dt.Rows[row][currentColumn] = "1";

                string record = dt.Rows[row]["record"].ObjToString();

                DataRow[] dRow = dx.Select("record='" + record + "'");

                if (dRow.Length > 0)
                {
                    dRow[0]["mod"] = "Y";
                    dRow[0][currentColumn] = dt.Rows[row][currentColumn].ObjToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** Updating CheckBoxes for Contract " + workContract + " Error " + ex.Message.ToString());
            }

            funModified = true;
            btnSaveAll.Show();
            btnSaveAll.Refresh();
        }
        /****************************************************************************************/
        private string currentColumn = "";
        private void gridMainDep_MouseDown(object sender, MouseEventArgs e)
        {
            //            var hitInfo = gridMainDep.CalcHitInfo(e.Location);
            var hitInfo = GetCurrentGridView().CalcHitInfo(e.Location);
            if (hitInfo.InRowCell)
            {
                int rowHandle = hitInfo.RowHandle;
                GridColumn column = hitInfo.Column;
                currentColumn = column.FieldName.Trim();
            }
        }
        /****************************************************************************************/
        private void chkLegal_EditValueChanged(object sender, EventArgs e)
        {
            DataTable dt = masterDt.Copy();
            dt.Columns.Add("MODD");
            string str = "";
            string relationship = "";
            for (int i = 0; i < chkLegal.Properties.Items.Count; i++)
            {
                if (chkLegal.Properties.Items[i].CheckState == CheckState.Checked)
                {
                    str = chkLegal.Properties.Items[i].Value.ObjToString().ToUpper();
                    for (int j = 0; j < dt.Rows.Count; j++)
                    {
                        relationship = dt.Rows[j]["depRelationship"].ObjToString().ToUpper();
                        if (relationship.Trim().ToUpper() == str)
                            dt.Rows[j]["MODD"] = "Y";
                    }
                }
            }
            for (int i = dt.Rows.Count - 1; i >= 0; i--)
            {
                str = dt.Rows[i]["MODD"].ObjToString();
                if (str != "Y")
                    dt.Rows.RemoveAt(i);
            }
            dgvLegal.DataSource = dt;
        }
        /***********************************************************************************************/
        private void LoadDBTable(string dbTable, string dbField, DevExpress.XtraEditors.Repository.RepositoryItemComboBox combo)
        {
            if (String.IsNullOrWhiteSpace(dbTable))
                return;
            if (dbTable.ToUpper() == "NONE")
            {
                combo.Items.Clear();
                return;
            }
            DataTable rx = G1.get_db_data("Select * from `" + dbTable + "`;");

            if (dbTable.ToUpper() == "REF_RELATIONS")
            {
                DataView tempview = rx.DefaultView;
                tempview.Sort = "relationship asc";
                rx = tempview.ToTable();
            }
            combo.Items.Clear();

            string name = "";
            for (int i = 0; i < rx.Rows.Count; i++)
            {
                name = rx.Rows[i][dbField].ToString().Trim();
                if (String.IsNullOrWhiteSpace(name))
                    continue;
                combo.Items.Add(name);
            }
        }
        /****************************************************************************************/
        private void addSignatureToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                DataRow dr = gridMainLegal.GetFocusedDataRow();
                int rowHandle = gridMainLegal.FocusedRowHandle;
                int row = gridMainLegal.GetDataSourceRowIndex(rowHandle);
                DataTable dt = (DataTable)dgvLegal.DataSource;
                string lastName = dt.Rows[row]["depLastName"].ObjToString();
                string firstName = dt.Rows[row]["depFirstName"].ObjToString();
                string name = firstName + " " + lastName;
                string record = dt.Rows[row]["record"].ObjToString();

                Image emptyImage = new Bitmap(1, 1);
                Byte[] bytes = dt.Rows[row]["signature"].ObjToBytes();
                Image myImage = emptyImage;
                if (bytes != null)
                    myImage = G1.byteArrayToImage(bytes);

                using (SignatureForm signatureForm = new SignatureForm("Enter Signature for " + name, myImage))
                {
                    if (signatureForm.ShowDialog() == DialogResult.OK)
                    {
                        Image signature = signatureForm.SignatureResult;
                        if (signature != null)
                        {
                            ImageConverter converter = new ImageConverter();
                            bytes = (byte[])converter.ConvertTo(signature, typeof(byte[]));
                            G1.update_blob("relatives", "record", record, "signature", bytes);
                            DateTime date = DateTime.Now;
                            string dateStr = date.ToString("MM/dd/yyyy");
                            string sigTime = date.ToString("yyyy-MM-dd HH:mm:ss");
                            G1.update_db_table("relatives", "record", record, new string[] { "signatureDate", dateStr, "sigTime", sigTime });
                            LoadFamily();
                            dt.Rows[row]["signature"] = bytes;
                            dgvLegal.DataSource = dt;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }
        /****************************************************************************************/
        private void btnSaveAll_Click(object sender, EventArgs e)
        {
            SaveRelatives();
            funModified = false;
            if (otherModified)
            {
                DataTable dt = (DataTable)dgv6.DataSource;
                SaveOtherData(workContract, dt, workFuneral);
                otherModified = false;
            }
            btnSaveAll.Hide();
        }
        /****************************************************************************************/
        private DataTable trackingDt = null;
        private DataTable trackDt = null;
        RepositoryItemComboBox ciLookup = new RepositoryItemComboBox();
        /****************************************************************************************/
        private void ReloadTrack()
        {
            if (trackDt != null)
            {
                trackDt.Rows.Clear();
                trackDt.Dispose();
                trackDt = null;
            }
            trackDt = G1.get_db_data("Select * from `track`;");
        }
        /****************************************************************************************/
        private void LoadOtherData()
        {
            trackingDt = G1.get_db_data("Select * from `tracking`;");
            trackDt = G1.get_db_data("Select * from `track`;");
            ciLookup.SelectedIndexChanged += CiLookup_SelectedIndexChanged;
            ciLookup.KeyPress += CiLookup_KeyPress;

            string dbfield = "";
            string data = "";
            DataRow[] dR = null;
            string cmd = "Select * from `cust_extended_layout` WHERE `group` <> 'Vital Statistics' ORDER BY `order`;";
            //            string cmd = "Select * from `cust_extended_layout` ORDER by `order`;";
            DataTable dx = G1.get_db_data(cmd);
            dx.Columns.Add("num");
            dx.Columns.Add("mod");
            dx.Columns.Add("data");
            dx.Columns.Add("add");
            dx.Columns.Add("edit");
            cmd = "Select * from `fcust_extended` where `contractNumber` = '" + workContract + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count > 0)
            {
                int groupNumber = 1;
                string oldGroup = "";
                string group = "";
                string type = "";
                string help = "";
                for (int i = 0; i < dx.Rows.Count; i++)
                {
                    group = dx.Rows[i]["group"].ObjToString();
                    if (String.IsNullOrWhiteSpace(oldGroup))
                        oldGroup = group;
                    if (String.IsNullOrWhiteSpace(group))
                        group = oldGroup;
                    if (group != oldGroup)
                        groupNumber++;
                    oldGroup = group;
                    group = groupNumber.ToString() + ". " + group;
                    dx.Rows[i]["group"] = group;
                    dbfield = dx.Rows[i]["dbfield"].ObjToString();
                    if (!String.IsNullOrWhiteSpace(dbfield))
                    {
                        if (G1.get_column_number(dt, dbfield) >= 0)
                        {
                            data = dt.Rows[0][dbfield].ObjToString();
                            dx.Rows[i]["data"] = data;
                        }
                    }
                    dR = trackingDt.Select("tracking='" + dbfield + "'");
                    if (dR.Length > 0)
                        dx.Rows[i]["help"] = "Tracking";
                    else
                    {
                        help = dx.Rows[i]["help"].ObjToString();
                        type = dx.Rows[i]["type"].ObjToString();
                        if (type.ToUpper() == "DATE" && String.IsNullOrWhiteSpace(help))
                            dx.Rows[i]["help"] = "Select Date";
                        else if (type.ToUpper() == "FULLDATE" && String.IsNullOrWhiteSpace(help))
                            dx.Rows[i]["help"] = "Select Date";
                        else if (type.ToUpper() == "DAY" && String.IsNullOrWhiteSpace(help))
                            dx.Rows[i]["help"] = "Select the Day of the Week";
                    }
                }
            }
            else
            {
                int groupNumber = 1;
                string oldGroup = "";
                string group = "";
                for (int i = 0; i < dx.Rows.Count; i++)
                {
                    group = dx.Rows[i]["group"].ObjToString();
                    if (String.IsNullOrWhiteSpace(oldGroup))
                        oldGroup = group;
                    if (String.IsNullOrWhiteSpace(group))
                        group = oldGroup;
                    if (group != oldGroup)
                        groupNumber++;
                    oldGroup = group;
                    group = groupNumber.ToString() + ". " + group;
                    dx.Rows[i]["group"] = group;
                    dbfield = dx.Rows[i]["dbfield"].ObjToString();
                    if (!String.IsNullOrWhiteSpace(dbfield))
                    {
                        if (G1.get_column_number(dt, dbfield) >= 0)
                        {
                            if (dt.Rows.Count > 0)
                            {
                                data = dt.Rows[0][dbfield].ObjToString();
                                dx.Rows[i]["data"] = data;
                            }
                        }
                    }
                    dR = trackingDt.Select("tracking='" + dbfield + "'");
                    if (dR.Length > 0)
                        dx.Rows[i]["help"] = "Tracking";
                }
            }
            gridMain6.Columns["num"].Visible = false;
            G1.NumberDataTable(dx);
            dgv6.DataSource = dx;
            otherModified = false;
            gridMain6.ExpandAllGroups();
        }
        /***************************************************************************************/
        private void CiLookup_KeyPress(object sender, KeyPressEventArgs e)
        {
            //if (isProtected)
            //{
            //    e.Handled = true;
            //}
        }
        /***************************************************************************************/
        private void CiLookup_SelectedIndexChanged(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv6.DataSource;
            DataRow dr = gridMain6.GetFocusedDataRow();
            int rowhandle = gridMain6.FocusedRowHandle;
            int row = gridMain6.GetDataSourceRowIndex(rowhandle);

            string help = dt.Rows[row]["help"].ObjToString();
            string dbField = dt.Rows[row]["dbField"].ObjToString();
            string str = "";

            ComboBoxEdit combo = (ComboBoxEdit)sender;
            //            string what = combo.Text.Trim().ToUpper();
            string what = combo.Text.Trim();
            dr["data"] = what;

            funModified = true;
            btnSaveAll.Show();

            if (help.ToUpper() == "TRACKING")
            {
                DataRow[] dR = null;
                string cmd = "reference LIKE '" + dbField + "~%'";
                DataRow[] dRows = dt.Select(cmd);
                if (dRows.Length > 0)
                {
                    string[] Lines = null;
                    string field = "";
                    string answer = "";
                    for (int i = 0; i < dRows.Length; i++)
                    {
                        Lines = dRows[i]["reference"].ObjToString().Split('~');
                        if (Lines.Length <= 1)
                            continue;
                        field = Lines[1].Trim();
                        dbField = FixUsingFieldData(dbField);

                        dR = trackDt.Select("tracking='" + dbField.Trim() + "' AND answer='" + what.Trim() + "' AND ( location='" + EditCust.activeFuneralHomeName + "' OR location='All' )");
                        answer = ProcessReference(dR, field);
                        dRows[i]["data"] = answer.Trim();
                        dRows[i]["mod"] = "Y";
                        //if (dR.Length > 0)
                        //{
                        //    Lines = field.Split('+');
                        //    answer = "";
                        //    for (int j = 0; j < Lines.Length; j++)
                        //    {
                        //        field = Lines[j].Trim();
                        //        try
                        //        {
                        //            if (!String.IsNullOrWhiteSpace(field))
                        //            {
                        //                str = dR[0][field].ObjToString();
                        //                answer += str + " ";
                        //            }
                        //        }
                        //        catch ( Exception ex )
                        //        {
                        //            if (field == ",")
                        //                answer = answer.Trim();
                        //            answer += field;
                        //            if (field == ",")
                        //                answer += " ";
                        //        }
                        //    }
                        //    dRows[i]["data"] = answer.Trim();
                        //    dRows[i]["mod"] = "Y";
                        //}
                    }
                }
                dt.AcceptChanges();
            }
        }
        /***************************************************************************************/
        private string ProcessReference(DataRow[] dR, string field, int index = 0)
        {
            if (dR.Length <= 0)
                return "";
            string answer = "";
            if (String.IsNullOrWhiteSpace(field))
                return answer;
            try
            {
                string[] Lines = null;
                if (field.IndexOf("~") >= 0)
                {
                    Lines = field.Split('~');
                    if (Lines.Length <= 1)
                        return answer;
                    field = Lines[1];
                }

                if (field.IndexOf("+") < 0)
                    answer = dR[index][field].ObjToString();
                else
                {
                    Lines = field.Split('+');
                    string str = "";
                    for (int j = 0; j < Lines.Length; j++)
                    {
                        field = Lines[j].Trim();
                        try
                        {
                            if (!String.IsNullOrWhiteSpace(field))
                            {
                                str = dR[index][field].ObjToString();
                                answer += str + " ";
                            }
                        }
                        catch (Exception ex)
                        {
                            if (field == ",")
                                answer = answer.Trim();
                            answer += field;
                            if (field == ",")
                                answer += " ";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return answer;
        }
        /***************************************************************************************/
        private void CiLookup_DataChanged(string what)
        {
            DataTable dt = (DataTable)dgv6.DataSource;
            DataRow dr = gridMain6.GetFocusedDataRow();
            int rowhandle = gridMain6.FocusedRowHandle;
            int row = gridMain6.GetDataSourceRowIndex(rowhandle);

            string help = dt.Rows[row]["help"].ObjToString();
            string dbField = dt.Rows[row]["dbField"].ObjToString();

            //ComboBoxEdit combo = (ComboBoxEdit)sender;
            //string what = combo.Text.Trim();
            dr["data"] = what;

            funModified = true;
            btnSaveAll.Show();

            if (help.ToUpper() == "TRACKING")
            {
                DataRow[] dR = null;
                string cmd = "reference LIKE '" + dbField + "~%'";
                DataRow[] dRows = dt.Select(cmd);
                if (dRows.Length > 0)
                {
                    string[] Lines = null;
                    string field = "";
                    string answer = "";
                    for (int i = 0; i < dRows.Length; i++)
                    {
                        Lines = dRows[i]["reference"].ObjToString().Split('~');
                        if (Lines.Length <= 1)
                            continue;
                        field = Lines[1].Trim();
                        dR = trackDt.Select("answer='" + what + "' AND location='" + EditCust.activeFuneralHomeName + "'");
                        if (dR.Length > 0)
                        {
                            answer = ProcessReference(dR, field);
                            //answer = dR[0][field].ObjToString();
                            dRows[i]["data"] = answer;
                            dRows[i]["mod"] = "Y";
                        }
                        else
                        {
                            dRows[i]["data"] = "";
                            dRows[i]["mod"] = "";
                        }
                    }
                }
                dt.AcceptChanges();
            }
        }
        /****************************************************************************************/
        private void CiLookup_SelectedIndexChangedx(object sender, EventArgs e)
        {
            if (loading)
                return;
            //DataTable dt = (DataTable)dgv6.DataSource;
            //DataRow dr = gridMain6.GetFocusedDataRow();
            //int rowhandle = gridMain6.FocusedRowHandle;
            //int row = gridMain6.GetDataSourceRowIndex(rowhandle);

            ComboBoxEdit combo = (ComboBoxEdit)sender;
            string what = combo.Text.Trim().ToUpper();
            //if ( dgv4.Visible )
            //{
            //    DataTable dt = (DataTable)dgv4.DataSource;
            //    DataRow dr = gridMain4.GetFocusedDataRow();
            //    int rowhandle = gridMain4.FocusedRowHandle;
            //    int row = gridMain4.GetDataSourceRowIndex(rowhandle);
            //    string prefix = "";
            //    string fname = "";
            //    string mi = "";
            //    string lname = "";
            //    string suffix = "";
            //    G1.ParseOutName(what, ref prefix, ref fname, ref lname, ref mi, ref suffix);
            //    loading = true;
            //    dr["depPrefix"] = prefix;
            //    dr["depFirstName"] = fname;
            //    dr["depMI"] = mi;
            //    dr["depLastName"] = lname;
            //    dr["depSuffix"] = suffix;
            //    combo.Text = lname;
            //    loading = false;
            //}
        }
        /****************************************************************************************/
        private bool specialLoading = false;
        private void gridMain6_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            if (specialLoading)
                return;
            DataTable dt = (DataTable)dgv6.DataSource;
            DataRow dr = gridMain6.GetFocusedDataRow();
            dr["mod"] = "Y";
            otherModified = true;
            funModified = true;
            btnSaveAll.Show();
            GridColumn currCol = gridMain6.FocusedColumn;
            currentColumn = currCol.FieldName;

            string what = dr["data"].ObjToString();
            if (String.IsNullOrWhiteSpace(what))
                return;

            DataTable dt6 = (DataTable)dgv6.DataSource;
            int rowHandle = gridMain6.FocusedRowHandle;
            int row = gridMain6.GetDataSourceRowIndex(rowHandle);
            string field = dt6.Rows[row]["field"].ObjToString();
            string record = dt6.Rows[row]["record"].ObjToString();
            string dbField = dt6.Rows[row]["dbField"].ObjToString();
            string tract = dt6.Rows[row]["help"].ObjToString().ToUpper();
            string reference = dt6.Rows[row]["reference"].ObjToString();
            //if ( field == "Service Clergy" )
            //{
            //}

            DataRow[] dRows = null;
            DataRow[] dR = null;
            DataTable dx = null;
            string cmd = "";

            try
            {
                if (gridMain6.Columns[currentColumn].ColumnEdit != null || tract.ToUpper() == "TRACKING")
                {
                    string answers = "";
                    bool found = false;
                    for (int i = 0; i < ciLookup.Items.Count; i++)
                    {
                        answers = ciLookup.Items[i].ObjToString();
                        if (what.Trim().ToUpper() == answers.Trim().ToUpper())
                        {
                            found = true;
                        }
                    }
                    if (!found)
                    {
                        dr["add"] = "+";
                        dr["edit"] = "E";
                    }
                    else
                    {
                        dr["add"] = "";
                        dr["edit"] = "";
                    }
                    bool accepted = false;
                    if (!found)
                    {
                        if (isProtected)
                        {
                            MessageBox.Show("***SORRY*** Field is protected.\nYou must choose from the dropdown!", "Data Entry Problem Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                            dr["data"] = "";
                            dr["add"] = "";
                            dr["edit"] = "";
                            return;
                        }
                        using (FuneralDemo funDemo = new FuneralDemo(field, what))
                        {
                            funDemo.ShowDialog();
                            if (funDemo.DialogResult == System.Windows.Forms.DialogResult.OK)
                            {
                                string address = funDemo.FireEventFunDemo("address");
                                string city = funDemo.FireEventFunDemo("city");
                                string county = funDemo.FireEventFunDemo("county");
                                string state = funDemo.FireEventFunDemo("state");
                                string zip = funDemo.FireEventFunDemo("zip");
                                string phone = funDemo.FireEventFunDemo("phone");
                                if (workDt6 == null)
                                {
                                    if (trackDt == null)
                                        trackDt = G1.get_db_data("Select * from `track`;");
                                    workDt6 = trackDt.Clone();
                                }
                                dR = workDt6.Select("tracking='" + field + "' and location='" + EditCust.activeFuneralHomeName + "'");
                                if (dR.Length <= 0)
                                {
                                    DataRow d = workDt6.NewRow();
                                    d["tracking"] = field;
                                    d["answer"] = what;
                                    d["address"] = address;
                                    d["city"] = city;
                                    d["county"] = county;
                                    d["state"] = state;
                                    d["zip"] = zip;
                                    d["phone"] = phone;
                                    d["location"] = EditCust.activeFuneralHomeName;
                                    workDt6.Rows.Add(d);
                                }
                                else
                                {
                                    dR[0]["answer"] = what;
                                    dR[0]["address"] = address;
                                    dR[0]["city"] = city;
                                    dR[0]["county"] = county;
                                    dR[0]["state"] = state;
                                    dR[0]["zip"] = zip;
                                    dR[0]["phone"] = phone;
                                    dR[0]["location"] = EditCust.activeFuneralHomeName;
                                }

                                dbField = FixUsingFieldData(dbField);
                                //string record2 = G1.create_record("track", "zip", "-1");
                                //if (G1.BadRecord("track", record2))
                                //{
                                //    MessageBox.Show("***ERROR*** Creating Track Record for Location ");
                                //    return;
                                //}
                                //G1.update_db_table("track", "record", record2, new string[] { "tracking", dbField, "location", EditCust.activeFuneralHomeName, "answer", what, "address", address, "city", city, "state", state, "zip", zip });
                                ProcessDataChanged(dt6, field, what);
                                ReloadTrack();
                                gridMain6_ShownEditor(null, null);
                                accepted = true;
                            }
                        }
                        if (!accepted)
                        {
                            if (reference.IndexOf("~") < 0)
                                return;
                            string[] Lines = reference.Split('~');
                            if (Lines.Length >= 2)
                            {
                                try
                                {
                                    string majorField = Lines[0].Trim();
                                    string myfield = Lines[1].Trim();
                                    dR = dt.Select("dbField = '" + majorField + "'");
                                    if (dR.Length > 0)
                                    {
                                        string answer = dR[0]["data"].ObjToString();
                                        dR = trackDt.Select("tracking='" + majorField + "' AND answer='" + answer + "' AND location='" + EditCust.activeFuneralHomeName + "'");
                                        if (dR.Length > 0)
                                        {
                                            record = dR[0]["record"].ObjToString();
                                            dR[0][field] = what;
                                            G1.update_db_table("track", "record", record, new string[] { field, what });
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                }
                            }
                        }
                    }
                    else
                        ProcessDataChanged(dt6, field, what);
                }
                else
                {
                    if (reference.IndexOf("~") < 0)
                    {
                        return;
                    }
                    string[] Lines = reference.Split('~');
                    if (Lines.Length >= 2)
                    {
                        try
                        {
                            string majorField = Lines[0].Trim();
                            string myfield = Lines[1].Trim();
                            dR = dt.Select("dbField = '" + majorField + "'");
                            if (dR.Length > 0)
                            {
                                string answer = dR[0]["data"].ObjToString();
                                dR = trackDt.Select("tracking='" + majorField + "' AND answer='" + answer + "' AND location='" + EditCust.activeFuneralHomeName + "'");
                                if (dR.Length > 0)
                                {
                                    record = dR[0]["record"].ObjToString();
                                    dR[0][myfield] = what;
                                    G1.update_db_table("track", "record", record, new string[] { myfield, what });
                                }
                            }
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
        }
        /****************************************************************************************/
        private string FixUsingFieldData(string field)
        {
            string newField = field;
            string cmd = "Select * from `tracking` where `tracking` = '" + field + "';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count > 0)
            {
                string useData = dx.Rows[0]["using"].ObjToString();
                if (!String.IsNullOrWhiteSpace(useData))
                    newField = useData;
            }
            return newField;
        }
        /****************************************************************************************/
        private void ProcessDataChanged(DataTable dt6, string field, string what)
        {
            string cmd = "";
            DataTable dx = null;
            DataRow[] dRows = null;
            DataRow[] dR = null;
            string reference = "";
            string answer = "";

            if (trackDt == null)
                trackDt = G1.get_db_data("Select * from `track`;");
            if (workDt6 == null)
                workDt6 = trackDt.Clone();

            dR = trackDt.Select("tracking = '" + field + "' AND answer='" + what + "' AND location='" + EditCust.activeFuneralHomeName + "'");
            if (dR.Length <= 0)
                dR = workDt6.Select("tracking = '" + field + "' AND answer='" + what + "' AND location='" + EditCust.activeFuneralHomeName + "'");
            if (dR.Length <= 0)
                return;

            if (field.ToUpper().IndexOf("LOCATION") < 0)
            {
                cmd = "reference LIKE '" + field + "~%'";
                dRows = dt6.Select(cmd);
                if (dRows.Length > 0)
                {
                    string[] Lines = null;
                    for (int i = 0; i < dRows.Length; i++)
                    {
                        if (dR.Length > 0)
                        {
                            reference = dRows[i]["reference"].ObjToString();
                            answer = ProcessReference(dR, reference);
                            dRows[i]["data"] = answer;
                            dRows[i]["mod"] = "Y";
                        }
                    }
                }
                return;
            }

            specialLoading = true;
            string dbField = "";
            string newField = field.ToUpper().Replace("LOCATION", "ADDRESS");
            dRows = dt6.Select("field='" + newField + "'");
            if (dRows.Length > 0)
            {
                reference = dRows[0]["reference"].ObjToString();
                dbField = dRows[0]["dbField"].ObjToString();
                answer = ProcessReference(dR, reference);
                dRows[0]["data"] = answer;
                //                dRows[0]["data"] = dR[0]["address"].ObjToString();
            }

            newField = field.ToUpper().Replace("LOCATION", "CITY");
            dRows = dt6.Select("field='" + newField + "'");
            if (dRows.Length > 0)
            {
                reference = dRows[0]["reference"].ObjToString();
                dbField = dRows[0]["dbField"].ObjToString();
                answer = ProcessReference(dR, reference);
                dRows[0]["data"] = answer;
                //dRows[0]["data"] = dR[0]["city"].ObjToString();
            }

            newField = field.ToUpper().Replace("LOCATION", "STATE");
            dRows = dt6.Select("field='" + newField + "'");
            if (dRows.Length > 0)
            {
                reference = dRows[0]["reference"].ObjToString();
                dbField = dRows[0]["dbField"].ObjToString();
                answer = ProcessReference(dR, reference);
                dRows[0]["data"] = answer;
                //                dRows[0]["data"] = dR[0]["state"].ObjToString();
            }

            newField = field.ToUpper().Replace("LOCATION", "ZIP");
            dRows = dt6.Select("field='" + newField + "'");
            if (dRows.Length > 0)
            {
                reference = dRows[0]["reference"].ObjToString();
                dbField = dRows[0]["dbField"].ObjToString();
                answer = ProcessReference(dR, reference);
                dRows[0]["data"] = answer;
                //                dRows[0]["data"] = dR[0]["zip"].ObjToString();
            }

            specialLoading = false;
        }
        /****************************************************************************************/
        private void ProcessDataChangedx(DataTable dt6, string field, string what)
        {
            if (field.ToUpper().IndexOf("LOCATION") < 0)
                return;
            string cmd = "";
            DataTable dx = null;
            DataRow[] dRows = null;
            DataRow[] dR = null;
            string reference = "";

            cmd = "Select * from `track` where `answer` = '" + what + "' ";
            if (!String.IsNullOrWhiteSpace(EditCust.activeFuneralHomeName))
                cmd += " and `location` = '" + EditCust.activeFuneralHomeName + "' ";
            cmd += ";";
            dx = G1.get_db_data(cmd);
            if (dx.Rows.Count > 0)
            {
                specialLoading = true;
                string newField = field.ToUpper().Replace("LOCATION", "ADDRESS");
                dRows = dt6.Select("field='" + newField + "'");
                if (dRows.Length > 0)
                {
                    reference = dRows[0]["reference"].ObjToString();
                    dRows[0]["data"] = dx.Rows[0]["address"].ObjToString();
                }

                newField = field.ToUpper().Replace("LOCATION", "CITY");
                dRows = dt6.Select("field='" + newField + "'");
                if (dRows.Length > 0)
                    dRows[0]["data"] = dx.Rows[0]["city"].ObjToString();

                newField = field.ToUpper().Replace("LOCATION", "STATE");
                dRows = dt6.Select("field='" + newField + "'");
                if (dRows.Length > 0)
                    dRows[0]["data"] = dx.Rows[0]["state"].ObjToString();

                newField = field.ToUpper().Replace("LOCATION", "ZIP");
                dRows = dt6.Select("field='" + newField + "'");
                if (dRows.Length > 0)
                    dRows[0]["data"] = dx.Rows[0]["zip"].ObjToString();


                specialLoading = false;
            }
        }
        /****************************************************************************************/
        public static void ConfirmCustExtended(string workContract, string serviceId = "", string serviceDate = "", string custExtendedFile = "", string county = "", string insideCity = "", string arrangementDate = "", string arrangementTime = "" )
        {
            if (String.IsNullOrWhiteSpace(custExtendedFile))
                custExtendedFile = "fcust_extended";
            string cmd = "Select * from `" + custExtendedFile + "` where `contractNumber` = '" + workContract + "';";
            DataTable dx = G1.get_db_data(cmd);

            string record = "";
            string caseCreatedDate = "";
            if (dx.Rows.Count > 0)
            {
                record = dx.Rows[0]["record"].ObjToString();
                if (custExtendedFile.ToUpper() == "FCUST_EXTENDED")
                {
                    DateTime date = dx.Rows[0]["caseCreatedDate"].ObjToDateTime();
                    if (date.Year < 100)
                        date = DateTime.Now;
                    caseCreatedDate = date.ToString("yyyy-MM-dd HH:mm:ss");
                }
            }
            if (String.IsNullOrWhiteSpace(record) || record == "-1")
            {
                record = G1.create_record(custExtendedFile, "field", "-1");
                if (String.IsNullOrWhiteSpace(caseCreatedDate))
                    caseCreatedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            }
            if (G1.BadRecord(custExtendedFile, record))
                return;
            string trust = "";
            string loc = "";
            bool isFuneral = false;
            if (custExtendedFile.ToUpper() == "FCUST_EXTENDED")
                isFuneral = true;
            Trust85.decodeContractNumber(serviceId, isFuneral, ref trust, ref loc);

            DateTime sDate = serviceDate.ObjToDateTime();
            string srvDate = "";
            if (sDate.Year > 100)
                srvDate = sDate.ToString("MMMM dd, yyyy");


            G1.update_db_table(custExtendedFile, "record", record, new string[] { "contractNumber", workContract, "serviceId", serviceId, "serviceDate", serviceDate, "serviceLoc", loc, "srvDate", srvDate, "DECCOUNTY", county, "IN CITY LIMITS", insideCity, "caseCreatedDate", caseCreatedDate, "arrangementDate", arrangementDate, "arrangementTime", arrangementTime });
        }
        /****************************************************************************************/
        public static void SaveOtherData(string workContract, DataTable dt, bool funeral)
        {
            string custExtendedFile = "cust_extended";
            if (funeral)
                custExtendedFile = "fcust_extended";
            string cmd = "Select * from `" + custExtendedFile + "` where `contractNumber` = '" + workContract + "';";
            DataTable dx = G1.get_db_data(cmd);

            string record = "";
            if (dx.Rows.Count > 0)
                record = dx.Rows[0]["record"].ObjToString();
            if (String.IsNullOrWhiteSpace(record) || record == "-1")
                record = G1.create_record(custExtendedFile, "field", "-1");
            if (G1.BadRecord(custExtendedFile, record))
                return;
            G1.update_db_table(custExtendedFile, "record", record, new string[] { "contractNumber", workContract });

            string dbfield = "";
            string data = "";
            string mod = "";
            string myList = "";
            DateTime serviceDate = DateTime.Now;
            string mysqlDate = "";
            bool gotTracking = false;
            string tracking = "";
            string dropOnly = "";
            string addContact = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                mod = dt.Rows[i]["mod"].ObjToString();
                if (mod == "Y")
                {
                    tracking = dt.Rows[i]["tracking"].ObjToString();
                    if (tracking.ToUpper() == "T")
                        gotTracking = true;
                    dropOnly = dt.Rows[i]["dropOnly"].ObjToString();
                    addContact = dt.Rows[i]["addContact"].ObjToString();
                    dbfield = dt.Rows[i]["dbfield"].ObjToString();
                    if (String.IsNullOrWhiteSpace(dbfield) )
                        continue;
                    data = dt.Rows[i]["data"].ObjToString();
                    data = G1.protect_data(data);
                    if (G1.get_column_number(dx, dbfield) >= 0)
                    {
                        if (data.IndexOf(",") >= 0)
                        {
                            G1.update_db_table(custExtendedFile, "record", record, new string[] { dbfield, data });
                        }
                        else
                            myList += dbfield + "," + data + ",";
                        if (dbfield.ToUpper() == "SRVDATE")
                        {
                            serviceDate = data.ObjToDateTime();
                            mysqlDate = serviceDate.ToString("MM/dd/yyyy");
                            G1.update_db_table(custExtendedFile, "record", record, new string[] { "serviceDate", mysqlDate });
                        }
                    }
                }
            }
            //            myList = myList.TrimEnd(',');
            if (String.IsNullOrWhiteSpace(myList))
                return;
            try
            {
                myList = myList.Remove(myList.LastIndexOf(","), 1);
                if (!String.IsNullOrWhiteSpace(myList))
                {
                    G1.update_db_table(custExtendedFile, "record", record, myList);
                    if (gotTracking)
                        ProcessTracking(dt);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** Updating Extended Data for Contract " + workContract + " Error " + ex.Message.ToString());
            }
        }
        /****************************************************************************************/
        public static void ProcessTracking ( DataTable dt )
        {
            string dbfield = "";
            string data = "";
            string mod = "";
            string myList = "";
            string tracking = "";
            string dropOnly = "";
            string addContact = "";
            string cmd = "";
            string record = "";
            string[] Lines = null;
            string field = "";
            DataTable dx = null;
            string location = EditCust.activeFuneralHomeName;

            try
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    mod = dt.Rows[i]["mod"].ObjToString();
                    if (mod == "Y")
                    {
                        tracking = dt.Rows[i]["tracking"].ObjToString();
                        dropOnly = dt.Rows[i]["dropOnly"].ObjToString();
                        if (dropOnly == "1")
                            continue;
                        addContact = dt.Rows[i]["addContact"].ObjToString();
                        dbfield = dt.Rows[i]["dbfield"].ObjToString();
                        if (String.IsNullOrWhiteSpace(dbfield))
                            continue;
                        if (tracking.ToUpper() != "T")
                            continue;
                        data = dt.Rows[i]["data"].ObjToString();
                        data = G1.protect_data(data);
                        //location = dt.Rows[i]["location"].ObjToString();
                        //if (String.IsNullOrWhiteSpace(location))
                        //    location = EditCust.activeFuneralHomeName;

                        cmd = "Select * from `track` where `tracking` = '" + dbfield + "' AND `answer` = '" + data + "' ";
                        //if (!String.IsNullOrWhiteSpace(location))
                        //    cmd += " AND `location` = '" + location + "'";
                        cmd += ";";
                        dx = G1.get_db_data(cmd);
                        if ( dx.Rows.Count <= 0 )
                        {
                            record = G1.create_record("track", "answer", "-1");
                            if (G1.BadRecord("track", record))
                                return;
                            G1.update_db_table("track", "record", record, new string[] { "tracking", dbfield, "answer", data, "location", EditCust.activeFuneralHomeName });
                            dx = G1.get_db_data(cmd);
                        }
                        if (dx.Rows.Count > 0 )
                        {
                            myList = "";
                            record = dx.Rows[0]["record"].ObjToString();
                            DataRow[] dRows = dt.Select("reference LIKE '" + dbfield + "~%'");
                            if (dRows.Length > 0)
                            {
                                string reference = "";
                                for (int j = 0; j < dRows.Length; j++)
                                {
                                    data = dRows[j]["data"].ObjToString();
                                    reference = dRows[j]["reference"].ObjToString();
                                    Lines = reference.Split('~');
                                    if ( Lines.Length > 1 )
                                    {
                                        field = Lines[1].Trim();
                                        myList += field + "," + data + ",";
                                    }
                                }
                            }
                            if ( !String.IsNullOrWhiteSpace ( record ) && !String.IsNullOrWhiteSpace ( myList ))
                            {
                                myList = myList.Remove(myList.LastIndexOf(","), 1);
                                G1.update_db_table("track", "record", record, myList);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** Updating Trackingm Data Error " + ex.Message.ToString());
            }
        }
        /****************************************************************************************/
        private DataTable LoadEmptyRelatives()
        {
            string cmd = "Select * from `relatives` where `contractNumber` = 'XYZZYABC'";
            DataTable dt = G1.get_db_data(cmd);
            return dt;
        }
        /****************************************************************************************/
        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            TabPage current = (sender as TabControl).SelectedTab;
            //if (workLegal)
            //    return;
            addSignatureToolStripMenuItem.Enabled = false;
            //contextMenuStrip1.Enabled = false;
            if (current.Name.Trim().ToUpper() == "TABFAMILY")
            {
                chkLegal.Hide();
                lblSelect.Hide();
                pictureBox11.Show();
                pictureBox12.Show();
                picRowUp.Show();
                picRowDown.Show();
                addSignatureToolStripMenuItem.Enabled = false;
                lblFamily.Text = "All Family Members :";
                DataTable dt = (DataTable)dgvDependent.DataSource;
                if (dt == null)
                    dt = LoadEmptyRelatives();
                G1.NumberDataTable(dt);
            }
            else if (current.Name.Trim().ToUpper() == "TABLEGAL")
            {
                chkLegal.Show();
                lblSelect.Show();
                pictureBox11.Hide();
                pictureBox12.Hide();
                picRowUp.Show();
                picRowDown.Show();
                DataTable dt = (DataTable)dgvDependent.DataSource;
                if (dt == null)
                    dt = LoadEmptyRelatives();
                G1.NumberDataTable(dt);
                DataTable legDt = dt.Copy();
                SetupNextOfKin2(legDt);
                LoadSignatures(legDt);
                G1.sortTable(legDt, "legOrder", "ASC");
                G1.NumberDataTable(legDt);

                dgvLegal.DataSource = legDt;
                addSignatureToolStripMenuItem.Enabled = true;
                contextMenuStrip1.Enabled = true;
                lblFamily.Text = "Select Legal Members :";
            }
            else if (current.Name.Trim().ToUpper() == "TABFUNERALDATA")
            {
                chkLegal.Hide();
                lblSelect.Hide();
                pictureBox11.Hide();
                pictureBox12.Hide();
                picRowUp.Hide();
                picRowDown.Hide();
                addSignatureToolStripMenuItem.Enabled = false;
                lblFamily.Text = "Enter Other Funeral Data :";
            }
            else if (current.Name.Trim().ToUpper() == "TABPALLBEARERS")
            {
                chkLegal.Hide();
                lblSelect.Hide();
                pictureBox11.Show();
                pictureBox12.Show();
                picRowUp.Show();
                picRowDown.Show();
                addSignatureToolStripMenuItem.Enabled = false;
                lblFamily.Text = "Enter Pall Bearers :";
                MergePallBearers();
            }
            else if (current.Name.Trim().ToUpper() == "TABHONORARYPALLBEARERS")
            {
                chkLegal.Hide();
                lblSelect.Hide();
                pictureBox11.Show();
                pictureBox12.Show();
                picRowUp.Show();
                picRowDown.Show();
                addSignatureToolStripMenuItem.Enabled = false;
                lblFamily.Text = "Honorary Pall Bearers :";
                MergeHPB();
            }
            else if (current.Name.Trim().ToUpper() == "TABCLERGY")
            {
                chkLegal.Hide();
                lblSelect.Hide();
                pictureBox11.Show();
                pictureBox12.Show();
                picRowUp.Show();
                picRowDown.Show();
                addSignatureToolStripMenuItem.Enabled = false;
                lblFamily.Text = "Enter Clergy Members :";
            }
            else if (current.Name.Trim().ToUpper() == "TABMUSICIANS")
            {
                chkLegal.Hide();
                lblSelect.Hide();
                pictureBox11.Show();
                pictureBox12.Show();
                picRowUp.Show();
                picRowDown.Show();
                addSignatureToolStripMenuItem.Enabled = false;
                lblFamily.Text = "Enter Musicians :";
            }
            else if (current.Name.Trim().ToUpper() == "TABDISCLOSURES")
            {
                chkLegal.Hide();
                lblSelect.Hide();
                pictureBox11.Show();
                pictureBox12.Show();
                picRowUp.Show();
                picRowDown.Show();
                DataTable dt = (DataTable)dgv7.DataSource;
                SetupNextOfKin7(dt);
                addSignatureToolStripMenuItem.Enabled = false;
                lblFamily.Text = "Enter Disclosures :";
            }
        }
        /***********************************************************************************************/
        private void MergePallBearers()
        {
            DataTable dx = (DataTable)dgv2.DataSource;
            string cmd = "Select * from `relatives` where `contractNumber` = '" + workContract + "' AND `depRelationship` = 'PB'";
            DataTable dt = G1.get_db_data(cmd);
            dt.Columns.Add("add");
            dt.Columns.Add("edit");
            string record = "";
            int i = 0;
            DataRow[] dR = null;
            try
            {
                if (G1.get_column_number(dt, "found") < 0)
                    dt.Columns.Add("found");
                if (G1.get_column_number(dx, "found") < 0)
                    dx.Columns.Add("found");
                bool changed = false;
                for (i = 0; i < dt.Rows.Count; i++)
                {
                    record = dt.Rows[i]["record"].ObjToString();
                    dR = dx.Select("record='" + record + "'");
                    if (dR.Length > 0)
                        dt.Rows[i]["found"] = "F";
                    else
                        dt.Rows[i]["found"] = "X";
                }
                dR = dt.Select("found='X'");
                DataTable dd = dt.Clone();
                G1.ConvertToTable(dR, dd);
                for (i = 0; i < dd.Rows.Count; i++)
                {
                    G1.copy_dt_row(dd, i, dx, dx.Rows.Count);
                    changed = true;
                }
                for (i = 0; i < dx.Rows.Count; i++)
                    dx.Rows[i]["found"] = "";
                for (i = 0; i < dt.Rows.Count; i++)
                    dt.Rows[i]["found"] = "";
                for (i = 0; i < dx.Rows.Count; i++)
                {
                    record = dx.Rows[i]["record"].ObjToString();
                    if (String.IsNullOrWhiteSpace(record))
                        continue;
                    dR = dt.Select("record='" + record + "'");
                    if (dR.Length > 0)
                        dx.Rows[i]["found"] = "F";
                    else
                        dx.Rows[i]["found"] = "X";
                }
                for (i = dx.Rows.Count - 1; i >= 0; i--)
                {
                    if (dx.Rows[i]["found"].ObjToString() == "X")
                    {
                        record = dx.Rows[i]["record"].ObjToString();
                        G1.delete_db_table("relatives", "record", record);
                        dx.Rows.RemoveAt(i);
                        changed = true;
                    }
                }
                dt.Columns.Remove("found");
                dx.Columns.Remove("found");
                if (changed)
                {
                    G1.NumberDataTable(dx);
                    dgv2.DataSource = dx;
                    dgv2.RefreshDataSource();
                }
            }
            catch (Exception ex)
            {
            }
        }
        /***********************************************************************************************/
        private void MergeHPB()
        {
            DataTable dx = (DataTable)dgv3.DataSource;
            string cmd = "Select * from `relatives` where `contractNumber` = '" + workContract + "' AND `depRelationship` = 'HPB'";
            DataTable dt = G1.get_db_data(cmd);
            dt.Columns.Add("add");
            dt.Columns.Add("edit");
            string record = "";
            int i = 0;
            DataRow[] dR = null;
            try
            {
                if (G1.get_column_number(dt, "found") < 0)
                    dt.Columns.Add("found");
                if (G1.get_column_number(dx, "found") < 0)
                    dx.Columns.Add("found");
                bool changed = false;
                for (i = 0; i < dt.Rows.Count; i++)
                {
                    record = dt.Rows[i]["record"].ObjToString();
                    dR = dx.Select("record='" + record + "'");
                    if (dR.Length > 0)
                        dt.Rows[i]["found"] = "F";
                    else
                        dt.Rows[i]["found"] = "X";
                }
                dR = dt.Select("found='X'");
                DataTable dd = dt.Clone();
                G1.ConvertToTable(dR, dd);
                for (i = 0; i < dd.Rows.Count; i++)
                {
                    G1.copy_dt_row(dd, i, dx, dx.Rows.Count);
                    changed = true;
                }
                for (i = 0; i < dx.Rows.Count; i++)
                    dx.Rows[i]["found"] = "";
                for (i = 0; i < dt.Rows.Count; i++)
                    dt.Rows[i]["found"] = "";
                for (i = 0; i < dx.Rows.Count; i++)
                {
                    record = dx.Rows[i]["record"].ObjToString();
                    if (String.IsNullOrWhiteSpace(record))
                        continue;
                    dR = dt.Select("record='" + record + "'");
                    if (dR.Length > 0)
                        dx.Rows[i]["found"] = "F";
                    else
                        dx.Rows[i]["found"] = "X";
                }
                for (i = dx.Rows.Count - 1; i >= 0; i--)
                {
                    if (dx.Rows[i]["found"].ObjToString() == "X")
                    {
                        record = dx.Rows[i]["record"].ObjToString();
                        G1.delete_db_table("relatives", "record", record);
                        dx.Rows.RemoveAt(i);
                        changed = true;
                    }
                }
                dt.Columns.Remove("found");
                dx.Columns.Remove("found");
                if (changed)
                {
                    G1.NumberDataTable(dx);
                    dgv3.DataSource = dx;
                    dgv3.RefreshDataSource();
                }
            }
            catch (Exception ex)
            {
            }
        }
        /***********************************************************************************************/
        private void SetupNextOfKin2(DataTable dt)
        {
            DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit selectnew = this.repositoryItemCheckEdit5;
            selectnew.NullText = "";
            selectnew.ValueChecked = "1";
            selectnew.ValueUnchecked = "0";
            selectnew.ValueGrayed = "";
            if (dt == null)
                return;
            string text = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                text = dt.Rows[i]["nextOfKin"].ObjToString();
                if (!String.IsNullOrWhiteSpace(text))
                    dt.Rows[i]["nextOfKin"] = text;
                else
                    dt.Rows[i]["nextOfKin"] = "0";

                text = dt.Rows[i]["informant"].ObjToString();
                if (!String.IsNullOrWhiteSpace(text))
                    dt.Rows[i]["informant"] = text;
                else
                    dt.Rows[i]["informant"] = "0";

                text = dt.Rows[i]["purchaser"].ObjToString();
                if (!String.IsNullOrWhiteSpace(text))
                    dt.Rows[i]["purchaser"] = text;
                else
                    dt.Rows[i]["purchaser"] = "0";

                text = dt.Rows[i]["authEmbalming"].ObjToString();
                if (!String.IsNullOrWhiteSpace(text))
                    dt.Rows[i]["authEmbalming"] = text;
                else
                    dt.Rows[i]["purchaser"] = "0";

                text = dt.Rows[i]["deceased"].ObjToString();
                if (!String.IsNullOrWhiteSpace(text))
                    dt.Rows[i]["deceased"] = text;
                else
                    dt.Rows[i]["deceased"] = "0";
            }
        }
        /***********************************************************************************************/
        private void SetupNextOfKin7(DataTable dt)
        {
            DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit selectnew = this.repositoryItemCheckEdit8;
            selectnew.NullText = "";
            selectnew.ValueChecked = "1";
            selectnew.ValueUnchecked = "0";
            selectnew.ValueGrayed = "";
            if (dt == null)
                return;
            string text = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                text = dt.Rows[i]["nextOfKin"].ObjToString();
                if (!String.IsNullOrWhiteSpace(text))
                    dt.Rows[i]["nextOfKin"] = text;
                else
                    dt.Rows[i]["nextOfKin"] = "0";
            }
        }
        /****************************************************************************************/
        private void repositoryItemCheckEdit5_Click(object sender, EventArgs e)
        {
            repositoryItemCheckEdit1_Click(sender, e);
        }
        /****************************************************************************************/
        private void gridMainLegal_MouseDown(object sender, MouseEventArgs e)
        {
            var hitInfo = GetCurrentGridView().CalcHitInfo(e.Location);
            if (hitInfo.InRowCell)
            {
                int rowHandle = hitInfo.RowHandle;
                GridColumn column = hitInfo.Column;
                currentColumn = column.FieldName.Trim();
            }
        }
        /****************************************************************************************/
        private void repositoryItemCheckEdit6_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMainDep.GetFocusedDataRow();
            DataTable dx = (DataTable)dgvDependent.DataSource;
            int rowHandle = gridMainDep.FocusedRowHandle;
            int row = gridMainDep.GetDataSourceRowIndex(rowHandle);
            dr["Mod"] = "Y";
            dx.Rows[row]["mod"] = "Y";

            dx.Rows[row]["mod"] = "Y";
            dx.Rows[row][currentColumn] = dx.Rows[row][currentColumn].ObjToString();

            funModified = true;
            btnSaveAll.Show();
        }
        /****************************************************************************************/
        RepositoryItemLookUpEdit riLookup = new RepositoryItemLookUpEdit();
        /****************************************************************************************/
        private void gridMain6_CellValueChanging(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
        }
        /****************************************************************************************/
        private void RiLookup_EditValueChanged(object sender, EventArgs e)
        {
            string items = string.Empty;
            //foreach (CheckedListBoxItem item in riLookup.GetItems())
            //{
            //    if (item.CheckState == CheckState.Checked)
            //        items += item.Value.ObjToString() + "~";
            //}

            //DataTable dt = (DataTable)dgv.DataSource;
            //DataRow dr = gridMain.GetFocusedDataRow();
            //int rowhandle = gridMain.FocusedRowHandle;
            //int row = gridMain.GetDataSourceRowIndex(rowhandle);
            //dr["assignedAgents"] = items;
            //dt.Rows[row]["assignedAgents"] = items;
        }
        /****************************************************************************************/
        private string lastDb = "";
        private DataTable myDt = null;
        private bool isProtected = false;
        /****************************************************************************************/
        private void gridMain6_ShownEditor(object sender, EventArgs e)
        {
            GridColumn currCol = gridMain6.FocusedColumn;
            currentColumn = currCol.FieldName;
            if (currentColumn.ToUpper() != "DATA")
                return;
            int focusedRow = gridMain6.FocusedRowHandle;
            int row = gridMain6.GetDataSourceRowIndex(focusedRow);
            gridMain6.ClearSelection();
            gridMain6.SelectRow(focusedRow);
            gridMain6.FocusedRowHandle = row;
            DataRow dr = gridMain6.GetFocusedDataRow();
            string dbField = dr["dbfield"].ObjToString();
            string help = dr["reference"].ObjToString();
            string field = "";
            isProtected = false;

            ciLookup.Items.Clear();
            if (myDt == null)
            {
                myDt = new DataTable();
                myDt.Columns.Add("stuff");
            }
            myDt.Rows.Clear();

            DataTable clergyDt = null;

            string[] Lines = null;
            DataRow[] dR = trackingDt.Select("tracking='" + dbField + "'");
            if (dbField.ToUpper().IndexOf("CLERGY") >= 0)
                clergyDt = (DataTable)dgv4.DataSource;
            if (dR.Length <= 0)
            {
                string type = dr["type"].ObjToString();
                string cmd = "";
                if (help.Length > 0)
                    cmd = help.Substring(0, 1);
                if (cmd == "$")
                {
                    Lines = help.Split('=');
                    if (Lines.Length < 2)
                        return;
                    try
                    {
                        if (cmd == "$")
                        {
                            string db = Lines[0];
                            db = db.Replace("$", "");
                            cmd = "Select * from `" + db + "`;";
                            field = Lines[1];
                            if (field.ToUpper().IndexOf("/PROTECT") > 0)
                                isProtected = true;
                            field = Regex.Replace(field, "/Protect", "", RegexOptions.IgnoreCase);

                            DataTable dd = G1.get_db_data(cmd);

                            for (int i = 0; i < dd.Rows.Count; i++)
                                AddToMyDt(dd.Rows[i][field].ObjToString());
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }
            else
            {
                if (help.Trim().ToUpper() == "/PROTECT")
                    isProtected = true;
                string substitute = dR[0]["using"].ObjToString();
                if (!String.IsNullOrWhiteSpace(substitute))
                    dbField = substitute;
                if (!String.IsNullOrWhiteSpace(EditCust.activeFuneralHomeName))
                    dR = trackDt.Select("tracking='" + dbField + "' AND ( location='" + EditCust.activeFuneralHomeName + "' ) ");
                else
                    dR = trackDt.Select("tracking='" + dbField + "'");
                ciLookup.Items.Clear();
                for (int i = 0; i < dR.Length; i++)
                    AddToMyDt(dR[i]["answer"].ObjToString());
                dR = trackDt.Select("tracking='" + dbField + "' AND ( location='All' ) ");
                if (dR.Length > 0)
                {
                    for (int i = 0; i < dR.Length; i++)
                        AddToMyDt(dR[i]["answer"].ObjToString());
                }
                if (clergyDt != null)
                {
                    myDt.Rows.Clear();
                    string fName = "";
                    string lName = "";
                    string name = "";
                    string clergy = "";
                    bool found = false;
                    for (int i = 0; i < clergyDt.Rows.Count; i++)
                    {
                        fName = clergyDt.Rows[i]["depFirstName"].ObjToString();
                        lName = clergyDt.Rows[i]["depLastName"].ObjToString();
                        clergy = fName + " " + lName;
                        fName = clergyDt.Rows[i]["depPrefix"].ObjToString();
                        lName = clergyDt.Rows[i]["fullName"].ObjToString();
                        if (!String.IsNullOrWhiteSpace(fName))
                            clergy = fName + " " + lName;
                        else
                            clergy = lName;
                        found = false;
                        for (int j = 0; j < myDt.Rows.Count; j++)
                        {
                            name = myDt.Rows[j]["stuff"].ObjToString();
                            if (name == clergy)
                            {
                                found = true;
                                break;
                            }
                        }
                        if (!found)
                            AddToMyDt(clergy);
                    }
                }
            }

            if (myDt.Rows.Count <= 0)
            {
                gridMain6.Columns["data"].ColumnEdit = null;
                string type = dr["type"].ObjToString();
                if (type.ToUpper() == "DATE" || type.ToUpper() == "DAY" || type.ToUpper() == "FULLDATE")
                {
                    DataTable dt = (DataTable)dgv6.DataSource;

                    string str = dt.Rows[row]["data"].ObjToString();
                    DateTime myDate = DateTime.Now;
                    if (!String.IsNullOrWhiteSpace(str))
                        myDate = str.ObjToDateTime();
                    string title = dt.Rows[row]["field"].ObjToString();
                    using (GetDate dateForm = new GetDate(myDate, title))
                    {
                        dateForm.ShowDialog();
                        if (dateForm.DialogResult == System.Windows.Forms.DialogResult.OK)
                        {
                            myDate = dateForm.myDateAnswer;
                            if (type.ToUpper() == "DAY")
                                dt.Rows[row]["data"] = G1.DayOfWeekText(myDate);
                            else if (type.ToUpper() == "FULLDATE")
                                dt.Rows[row]["data"] = myDate.ToString("MMMM d, yyyy");
                            else
                                dt.Rows[row]["data"] = myDate.ToString("MM/dd/yyyy");
                            dt.Rows[row]["mod"] = "Y";
                            string dateField = dt.Rows[row]["field"].ObjToString();
                            dateField = dateField.ToUpper().Replace("DATE", "Day");
                            DataRow[] ddR = dt.Select("field='" + dateField + "'");
                            if (ddR.Length <= 0)
                            {
                                dateField = dateField.ToUpper().Replace("DAY", "DayDate");
                                ddR = dt.Select("field='" + dateField + "'");
                            }
                            if (ddR.Length > 0)
                            {
                                type = ddR[0]["type"].ObjToString();
                                if (type.ToUpper() == "DAY")
                                {
                                    ddR[0]["data"] = G1.DayOfWeekText(myDate);
                                    ddR[0]["mod"] = "Y";
                                }
                            }
                            gridMain6.RefreshData();
                            gridMain6.RefreshEditor(true);
                            funModified = true;
                            otherModified = true;
                            btnSaveAll.Show();
                            btnSaveAll.Refresh();
                        }
                    }
                }
                gridMain6.RefreshData();
                gridMain6.RefreshEditor(true);
            }
            else
            {
                if (lastDb != dbField)
                {
                    lastDb = dbField;
                    if (gridMain6.Columns["data"].ColumnEdit != null)
                    {
                        gridMain6.Columns["data"].ColumnEdit = null;
                    }
                }
                for (int i = 0; i < myDt.Rows.Count; i++)
                    ciLookup.Items.Add(myDt.Rows[i]["stuff"].ObjToString());

                gridMain6.Columns["data"].ColumnEdit = ciLookup;
                gridMain6.RefreshData();
                gridMain6.RefreshEditor(true);
            }
        }
        /****************************************************************************************/
        private void AddToMyDt(string data)
        {
            if (myDt == null)
            {
                myDt = new DataTable();
                myDt.Columns.Add("stuff");
            }
            DataRow dRow = myDt.NewRow();
            dRow["stuff"] = data;
            myDt.Rows.Add(dRow);
        }
        /****************************************************************************************/
        private void gridMain6_MouseDown(object sender, MouseEventArgs e)
        {
            var hitInfo = gridMain6.CalcHitInfo(e.Location);
            if (hitInfo.InRowCell)
            {
                int rowHandle = hitInfo.RowHandle;
                gridMain6.SelectRow(rowHandle);
                dgv6.RefreshDataSource();
                DataTable dt = (DataTable)dgv6.DataSource;

                GridColumn column = hitInfo.Column;
                string type = dt.Rows[rowHandle]["type"].ObjToString();
                currentColumn = column.FieldName.Trim();
                string data = dt.Rows[rowHandle][currentColumn].ObjToString();
                if (data.Trim() == "+")
                {
                    string field = dt.Rows[rowHandle]["dbfield"].ObjToString();
                    data = dt.Rows[rowHandle]["data"].ObjToString();
                    if (String.IsNullOrWhiteSpace(data))
                        return;
                    DataRow[] dR = trackingDt.Select("tracking='" + field + "'");
                    if (dR.Length > 0)
                    {
                        string u = dR[0]["using"].ObjToString();
                        if (!String.IsNullOrWhiteSpace(u))
                            field = u;
                    }
                    dR = trackDt.Select("tracking='" + field + "' AND answer='" + data + "' AND location='" + EditCust.activeFuneralHomeName + "'");
                    if (dR.Length <= 0)
                    {
                        string record = G1.create_record("track", "answer", "-1");
                        if (G1.BadRecord("track", record))
                            return;
                        G1.update_db_table("track", "record", record, new string[] { "tracking", field, "answer", data, "location", EditCust.activeFuneralHomeName });
                        string trackField = dt.Rows[rowHandle]["field"].ObjToString();
                        dR = workDt6.Select("tracking='" + trackField + "' and answer = '" + data + "' and location = '" + EditCust.activeFuneralHomeName + "'");
                        DataRow dRow = trackDt.NewRow();
                        dRow["tracking"] = field;
                        dRow["answer"] = data;
                        dRow["location"] = EditCust.activeFuneralHomeName;
                        dRow["record"] = record;
                        if (dR.Length > 0)
                        {
                            dRow["address"] = dR[0]["address"].ObjToString();
                            dRow["city"] = dR[0]["city"].ObjToString();
                            dRow["county"] = dR[0]["county"].ObjToString();
                            dRow["state"] = dR[0]["state"].ObjToString();
                            dRow["zip"] = dR[0]["zip"].ObjToString();
                            dRow["phone"] = dR[0]["phone"].ObjToString();
                            G1.update_db_table("track", "record", record, new string[] { "address", dR[0]["address"].ObjToString(), "city", dR[0]["city"].ObjToString(), "county", dR[0]["county"].ObjToString(), "state", dR[0]["state"].ObjToString(), "zip", dR[0]["zip"].ObjToString(), "phone", dR[0]["phone"].ObjToString() });
                        }
                        trackDt.Rows.Add(dRow);

                        field = dt.Rows[rowHandle]["dbfield"].ObjToString();
                        dR = trackingDt.Select("tracking='" + field + "'");
                        if (dR.Length > 0)
                        {
                            string u = dR[0]["using"].ObjToString();
                            if (!String.IsNullOrWhiteSpace(u))
                                field = u;
                        }
                        EditTracking trackForm = new EditTracking(field, EditCust.activeFuneralHomeName);
                        trackForm.ShowDialog();
                        trackDt = G1.get_db_data("Select * from `track`");
                        string cmd = "Select * from `track` WHERE `record` = '" + record + "';";
                        DataTable dx = G1.get_db_data(cmd);
                        gridMain6_ShownEditor(null, null);
                        string what = data;
                        CiLookup_DataChanged(what);
                        string newAnswer = what;
                        if (!String.IsNullOrWhiteSpace(newAnswer))
                        {
                            dt.Rows[rowHandle]["data"] = newAnswer;
                            cmd = "Select * from `track` where `tracking` = '" + field + "' and `location` = '" + EditCust.activeFuneralHomeName + "' AND `answer` = '" + newAnswer + "';";
                            dx = G1.get_db_data(cmd);
                            if (dx.Rows.Count <= 0)
                            {
                                dt.Rows[rowHandle]["add"] = "+";
                                dt.Rows[rowHandle]["edit"] = "E";
                            }
                            dgv6.RefreshDataSource();
                        }
                    }
                    else
                        dt.Rows[rowHandle][currentColumn] = "";
                    dgv6.RefreshDataSource();
                }
                else if (data.Trim() == "E")
                {
                    gridMain6_DoubleClick(null, null);

                    //string field = dt.Rows[rowHandle]["dbfield"].ObjToString();
                    //DataRow[] dR = trackingDt.Select("tracking='" + field + "'");
                    //if (dR.Length > 0)
                    //{
                    //    string u = dR[0]["using"].ObjToString();
                    //    if (!String.IsNullOrWhiteSpace(u))
                    //        field = u;
                    //}
                    //EditTracking trackForm = new EditTracking(field, EditCust.activeFuneralHomeName);
                    //trackForm.ShowDialog();
                    //trackDt = G1.get_db_data("Select * from `track`");
                    //gridMain6_ShownEditor(null, null);
                    //string newAnswer = EditTracking.trackingSelection;
                    //if (!String.IsNullOrWhiteSpace(newAnswer))
                    //{
                    //    dt.Rows[rowHandle]["data"] = newAnswer;
                    //    dt.Rows[rowHandle]["add"] = "";
                    //    dgv6.RefreshDataSource();
                    //}
                }
                //else
                //{
                //    gridMain6.RefreshEditor(true);
                //}
            }
            //else
            //{
            //    gridMain6.RefreshEditor(true);
            //}
        }
        /****************************************************************************************/
        private void gridMain4_ShownEditor(object sender, EventArgs e)
        {
            GridColumn currCol = gridMain4.FocusedColumn;
            currentColumn = currCol.FieldName;
            if (currentColumn.ToUpper() != "FULLNAME")
                return;
            DataRow dr = gridMain4.GetFocusedDataRow();
            string dbField = "SRVClergy";
            DataRow[] dR = trackingDt.Select("tracking='" + dbField + "'");
            if (dR.Length <= 0)
            {
                gridMain4.Columns["fullName"].ColumnEdit = null;
                return;
            }

            dR = trackDt.Select("tracking='" + dbField + "' AND location='" + EditCust.activeFuneralHomeName + "'");
            ciLookup.Items.Clear();
            for (int i = 0; i < dR.Length; i++)
                ciLookup.Items.Add(dR[i]["answer"].ObjToString());
            try
            {
                gridMain4.Columns["fullName"].ColumnEdit = ciLookup;
            }
            catch (Exception ex)
            {
            }
        }
        /****************************************************************************************/
        private void gridMain4_MouseDown(object sender, MouseEventArgs e)
        {
            var hitInfo = gridMain4.CalcHitInfo(e.Location);
            if (hitInfo.InRowCell)
            {
                int rowHandle = hitInfo.RowHandle;
                dgv6.RefreshDataSource();
                DataTable dt = (DataTable)dgv4.DataSource;

                GridColumn column = hitInfo.Column;
                currentColumn = column.FieldName.Trim();
                string data = dt.Rows[rowHandle][currentColumn].ObjToString();
                if (data.Trim() == "+")
                {
                    data = dt.Rows[rowHandle]["fullName"].ObjToString();
                    if (String.IsNullOrWhiteSpace(data))
                        return;
                    string title = dt.Rows[rowHandle]["depPrefix"].ObjToString();
                    if (!String.IsNullOrWhiteSpace(title))
                        data = title + " " + data;
                    string field = "SRVClergy";
                    DataRow[] dR = trackingDt.Select("tracking='" + field + "'");
                    if (dR.Length > 0)
                    {
                        string u = dR[0]["using"].ObjToString();
                        if (!String.IsNullOrWhiteSpace(u))
                            field = u;
                    }
                    dR = trackDt.Select("tracking='" + field + "' AND answer='" + data + "' AND location='" + EditCust.activeFuneralHomeName + "'");
                    if (dR.Length <= 0)
                    {
                        string record = G1.create_record("track", "answer", "-1");
                        if (G1.BadRecord("track", record))
                            return;
                        G1.update_db_table("track", "record", record, new string[] { "tracking", field, "answer", data, "location", EditCust.activeFuneralHomeName });
                        DataRow dRow = trackDt.NewRow();
                        dRow["tracking"] = field;
                        dRow["answer"] = data;
                        dRow["record"] = record;
                        trackDt.Rows.Add(dRow);
                    }
                    dt.Rows[rowHandle][currentColumn] = "";
                    dgv4.RefreshDataSource();
                }
                else if (data.Trim() == "E")
                {
                    string field = "SRVClergy";
                    DataRow[] dR = trackingDt.Select("tracking='" + field + "'");
                    if (dR.Length > 0)
                    {
                        string u = dR[0]["using"].ObjToString();
                        if (!String.IsNullOrWhiteSpace(u))
                            field = u;
                    }
                    EditTracking trackForm = new EditTracking(field, EditCust.activeFuneralHomeName);
                    trackForm.ShowDialog();
                    trackDt = G1.get_db_data("Select * from `track`");
                    gridMain4_ShownEditor(null, null);
                    string newAnswer = EditTracking.trackingSelection;
                    if (!String.IsNullOrWhiteSpace(newAnswer))
                    {
                        loading = true;
                        dt.Rows[rowHandle]["fullName"] = newAnswer;
                        dt.Rows[rowHandle]["add"] = "";
                        string cmd = "Select * from `track` where `tracking` = '" + field + "' and `location` = '" + EditCust.activeFuneralHomeName + "' AND `answer` = '" + newAnswer + "';";
                        DataTable dx = G1.get_db_data(cmd);
                        if (dx.Rows.Count <= 0)
                        {
                            dt.Rows[rowHandle]["add"] = "+";
                            dt.Rows[rowHandle]["edit"] = "E";
                        }
                        loading = false;
                        dgv4.RefreshDataSource();
                    }
                }
            }
        }
        /****************************************************************************************/
        private string BuildClergyName(DataTable dt, int rowHandle)
        {
            string prefix = dt.Rows[rowHandle]["depPrefix"].ObjToString();
            string firstName = dt.Rows[rowHandle]["depFirstName"].ObjToString();
            string miName = dt.Rows[rowHandle]["depMI"].ObjToString();
            string lastName = dt.Rows[rowHandle]["depLastName"].ObjToString();
            string suffix = dt.Rows[rowHandle]["depSuffix"].ObjToString();
            string data = prefix.Trim();
            if (!String.IsNullOrWhiteSpace(firstName))
                data += " " + firstName;
            if (!String.IsNullOrWhiteSpace(miName))
                data += " " + miName;
            if (!String.IsNullOrWhiteSpace(lastName))
                data += " " + lastName;
            if (!String.IsNullOrWhiteSpace(suffix))
                data += " " + suffix;
            data = data.TrimStart(' ');
            data = data.TrimEnd(' ');
            return data;
        }
        /****************************************************************************************/
        private void gridMain6_DoubleClick(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv6.DataSource;
            DataRow dr = gridMain6.GetFocusedDataRow();
            int rowHandle = gridMain6.FocusedRowHandle;
            GridColumn column = gridMain6.FocusedColumn;
            string columnName = column.FieldName.ObjToString();
            if (columnName.ToUpper() == "DATA")
                return;

            string field = dr["dbfield"].ObjToString();
            if (String.IsNullOrWhiteSpace(field))
                return;
            string cmd = "Select * from `tracking` where `tracking` = '" + field + "';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count <= 0)
                return;
            string trackUsing = dx.Rows[0]["using"].ObjToString();
            if (!String.IsNullOrWhiteSpace(trackUsing))
                field = trackUsing;
            EditTracking trackForm = new EditTracking(field, EditCust.activeFuneralHomeName);
            trackForm.ShowDialog();
            trackDt = G1.get_db_data("Select * from `track`");
            gridMain6_ShownEditor(null, null);
            string newAnswer = EditTracking.trackingSelection;
            if (!String.IsNullOrWhiteSpace(newAnswer))
            {
                string dbField = dr["dbField"].ObjToString().Trim();
                dt.Rows[rowHandle]["data"] = newAnswer;
                dt.Rows[rowHandle]["add"] = "";
                cmd = "Select * from `track` where `tracking` = '" + field + "' and `location` = '" + EditCust.activeFuneralHomeName + "' AND `answer` = '" + newAnswer + "';";
                dx = G1.get_db_data(cmd);
                if (dx.Rows.Count <= 0)
                {
                    dt.Rows[rowHandle]["add"] = "+";
                    dt.Rows[rowHandle]["edit"] = "E";
                }

                dt.Rows[rowHandle]["mod"] = "Y";
                otherModified = true;
                if (!String.IsNullOrWhiteSpace(dbField))
                {
                    try
                    {
                        DataRow[] dR = trackDt.Select("tracking='" + dbField + "' AND answer='" + newAnswer + "' AND location='" + EditCust.activeFuneralHomeName + "'");
                        if (dR.Length > 0)
                        {
                            DataRow[] dRows = dt.Select("reference LIKE '" + dbField + "~%'");
                            if (dRows.Length > 0)
                            {
                                string reference = "";
                                for (int i = 0; i < dRows.Length; i++)
                                {
                                    reference = dRows[i]["reference"].ObjToString();
                                    string answer = ProcessReference(dR, reference);
                                    dRows[i]["data"] = answer;
                                    dRows[i]["mod"] = "Y";
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                }
                dgv6.RefreshDataSource();
            }
        }
        /***********************************************************************************************/
        private void picRowUp_Click(object sender, EventArgs e)
        {
            if (dgv6.Visible)
                return;
            DevExpress.XtraGrid.GridControl dgv = null;
            DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gridMain = null;
            if (this.dgvDependent.Visible)
            {
                dgv = this.dgvDependent;
                gridMain = this.gridMainDep;
            }
            else if (this.dgvLegal.Visible)
            {
                dgv = this.dgvLegal;
                gridMain = this.gridMainLegal;
            }
            else if (this.dgv2.Visible)
            {
                dgv = this.dgv2;
                gridMain = this.gridMain2;
            }
            else if (this.dgv3.Visible)
            {
                dgv = this.dgv3;
                gridMain = this.gridMain3;
            }
            else if (this.dgv4.Visible)
            {
                dgv = this.dgv4;
                gridMain = this.gridMain4;
            }
            else if (this.dgv5.Visible)
            {
                dgv = this.dgv5;
                gridMain = this.gridMain5;
            }
            if (dgv == null)
                return;
            if (gridMain == null)
                return;

            DataTable dt = (DataTable)dgv.DataSource;
            if (dt.Rows.Count <= 0)
                return;
            DataRow dr = gridMain.GetFocusedDataRow();
            int rowHandle = gridMain.FocusedRowHandle;
            if (rowHandle == 0)
                return; // Already at the first row
            MoveRowUp(dt, rowHandle);
            //massRowsUp(gridMain, dt, rowHandle);
            dt.AcceptChanges();
            dgv.DataSource = dt;
            gridMain.ClearSelection();
            gridMain.SelectRow(rowHandle - 1);
            gridMain.FocusedRowHandle = rowHandle - 1;
            gridMain.RefreshData();
            dgv.Refresh();
            btnSaveAll.Show();
            if (this.dgvLegal.Visible)
                FixLegalOrder(dt);
            funModified = true;
        }
        /***************************************************************************************/
        private void FixLegalOrder(DataTable dt)
        {
            DataTable mainDt = (DataTable)this.dgvDependent.DataSource;
            string record = "";
            DataRow[] dRow = null;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                record = dt.Rows[i]["record"].ObjToString();
                dRow = mainDt.Select("record='" + record + "'");
                if (dRow.Length > 0)
                    dRow[0]["LegOrder"] = i;
            }
            this.dgvDependent.DataSource = mainDt;
        }
        /***************************************************************************************/
        private void MoveRowUp(DataTable dt, int row)
        {
            dt.AcceptChanges();
            if (G1.get_column_number(dt, "Count") < 0)
                dt.Columns.Add("Count", Type.GetType("System.Int32"));
            for (int i = 0; i < dt.Rows.Count; i++)
                dt.Rows[i]["Count"] = i.ToString();
            dt.Rows[row]["Count"] = (row - 1).ToString();
            dt.Rows[row - 1]["Count"] = row.ToString();
            G1.sortTable(dt, "Count", "asc");
            dt.Columns.Remove("Count");
            G1.NumberDataTable(dt);
        }
        /***********************************************************************************************/
        private void picRowDown_Click(object sender, EventArgs e)
        {
            if (dgv6.Visible)
                return;
            DevExpress.XtraGrid.GridControl dgv = null;
            DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gridMain = null;
            if (this.dgvDependent.Visible)
            {
                dgv = this.dgvDependent;
                gridMain = this.gridMainDep;
            }
            else if (this.dgvLegal.Visible)
            {
                dgv = this.dgvLegal;
                gridMain = this.gridMainLegal;
            }
            else if (this.dgv2.Visible)
            {
                dgv = this.dgv2;
                gridMain = this.gridMain2;
            }
            else if (this.dgv3.Visible)
            {
                dgv = this.dgv3;
                gridMain = this.gridMain3;
            }
            else if (this.dgv4.Visible)
            {
                dgv = this.dgv4;
                gridMain = this.gridMain4;
            }
            else if (this.dgv5.Visible)
            {
                dgv = this.dgv5;
                gridMain = this.gridMain5;
            }
            if (dgv == null)
                return;
            if (gridMain == null)
                return;

            DataTable dt = (DataTable)dgv.DataSource;
            if (dt.Rows.Count <= 0)
                return;
            DataRow dr = gridMain.GetFocusedDataRow();
            int rowHandle = gridMain.FocusedRowHandle;
            if (rowHandle == (dt.Rows.Count - 1))
                return; // Already at the last row
            MoveRowDown(dt, rowHandle);
            dt.AcceptChanges();
            dgv.DataSource = dt;
            gridMain.ClearSelection();
            gridMain.SelectRow(rowHandle + 1);
            gridMain.FocusedRowHandle = rowHandle + 1;
            gridMain.RefreshData();
            dgv.Refresh();
            btnSaveAll.Show();
            if (this.dgvLegal.Visible)
                FixLegalOrder(dt);
            funModified = true;
        }
        /***************************************************************************************/
        private void MoveRowDown(DataTable dt, int row)
        {
            dt.Columns.Add("Count", Type.GetType("System.Int32"));
            for (int i = 0; i < dt.Rows.Count; i++)
                dt.Rows[i]["Count"] = i.ToString();
            dt.Rows[row]["Count"] = (row + 1).ToString();
            dt.Rows[row + 1]["Count"] = row.ToString();
            G1.sortTable(dt, "Count", "asc");
            dt.Columns.Remove("Count");
            for (int i = 0; i < dt.Rows.Count; i++)
                dt.Rows[i]["Num"] = (i + 1).ToString();
        }
        /****************************************************************************************/
        private void repositoryItemCheckEdit8_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain7.GetFocusedDataRow();
            DataTable dt = (DataTable)dgv7.DataSource;
            int rowHandle = gridMain7.FocusedRowHandle;
            int row = gridMain7.GetDataSourceRowIndex(rowHandle);
            dr["Mod"] = "Y";
            dt.Rows[row]["mod"] = "Y";

            dt.Rows[row]["mod"] = "Y";
            //dt.Rows[row]["nextOfKin"] = dt.Rows[row]["nextOfKin"].ObjToString();

            string answer = dt.Rows[row]["nextOfKin"].ObjToString();
            if (answer == "1")
            {
                string field = dt.Rows[row]["depFirstName"].ObjToString();
                DataTable dx = G1.get_db_data("Select * from `disclosures` where `disclosure` = '" + field + "';");
                if (dx.Rows.Count > 0)
                {
                    string str = dx.Rows[0]["answer"].ObjToString();
                    if (!String.IsNullOrWhiteSpace(str))
                    {
                        dt.Rows[row]["depLastName"] = str;
                        dr["depLastName"] = str;
                    }
                }
            }

            dgv7.DataSource = dt;
            dgv7.RefreshDataSource();
            dgv7.Refresh();
            funModified = true;
            btnSaveAll.Show();
        }
        /****************************************************************************************/
        private void gridMain7_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            DataRow dr = gridMain7.GetFocusedDataRow();
            dr["mod"] = "Y";
            funModified = true;
            btnSaveAll.Show();
        }
        /****************************************************************************************/
        private void CheckAddNewRow()
        {
            DataTable dt = GetCurrentDataTable();
            int rowHandle = GetCurrentGridView().FocusedRowHandle;
            int row = GetCurrentGridView().GetDataSourceRowIndex(rowHandle);
            if (rowHandle >= (dt.Rows.Count - 1))
            {
                AddNewRow();
                GetCurrentGridView().FocusedRowHandle = rowHandle + 1;
                GetCurrentGridView().SelectRow(rowHandle + 1);
                if (GetCurrentGridView().VisibleColumns.Count > 0)
                {
                    GridColumn firstColumn = GetCurrentGridView().Columns["fullName"];
                    if (!firstColumn.Visible)
                    {
                        firstColumn = GetCurrentGridView().Columns["depFirstName"];
                        if (!firstColumn.Visible)
                            firstColumn = GetCurrentGridView().VisibleColumns[1];
                    }
                    GetCurrentGridView().FocusedColumn = GetCurrentGridView().Columns[firstColumn.FieldName];
                }
            }
            else
            {
                GetCurrentGridView().FocusedRowHandle = rowHandle + 1;
                GetCurrentGridView().SelectRow(rowHandle + 1);
            }
        }
        /****************************************************************************************/
        private bool cellChanging = false;
        private void gridMainDep_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (cellChanging)
                {
                    cellChanging = false;
                    return;
                }
                CheckAddNewRow();
                e.Handled = true;
                cellChanging = false;
                return;
            }
            cellChanging = true;
        }
        /****************************************************************************************/
        private void gridMainDep_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
                cellChanging = true;
        }
        /****************************************************************************************/
        private void gridMainDep_KeyPress(object sender, KeyPressEventArgs e)
        {
            Keys c = (Keys)e.KeyChar;
            if (c != Keys.Enter)
            {
                cellChanging = true;

            }
        }
        /****************************************************************************************/
        private void gridMainDep_CellValueChanging(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            cellChanging = true;
        }
        /****************************************************************************************/
        private void copyFromDeceasedAddressToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string cmd = "Select * from `fcustomers` where `contractNumber` = '" + workContract + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                return;
            string address1 = dt.Rows[0]["address1"].ObjToString();
            string address2 = dt.Rows[0]["address2"].ObjToString();
            string city = dt.Rows[0]["city"].ObjToString();
            string state = dt.Rows[0]["state"].ObjToString();
            string zip1 = dt.Rows[0]["zip1"].ObjToString();
            string zip2 = dt.Rows[0]["zip2"].ObjToString();

            if (!String.IsNullOrWhiteSpace(zip2))
                zip1 += "-" + zip2;

            DataTable dx = GetCurrentDataTable();

            DataRow dr = GetCurrentDataRow();
            if (dr == null)
                return;
            dr["address"] = address1 + address2;
            dr["city"] = city;
            dr["state"] = state;
            dr["zip"] = zip1;

            GetCurrentDataGrid().Refresh();
            gridMainDep_CellValueChanged(null, null);
        }
        /****************************************************************************************/
        private void gridMainLegal_CustomRowFilter(object sender, DevExpress.XtraGrid.Views.Base.RowFilterEventArgs e)
        {
            int row = e.ListSourceRow;
            //            DataTable dt = (DataTable)dgvDependent.DataSource;
            DataTable dt = GetCurrentDataTable();
            if (dt == null)
                return;
            try
            {
                string delete = dt.Rows[row]["mod"].ObjToString();
                if (delete.ToUpper() == "D")
                {
                    e.Visible = false;
                    e.Handled = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** Not Showing Deleted Legal Members for Contract " + workContract + " Error " + ex.Message.ToString());
            }
        }
        /****************************************************************************************/
        private void gridMainLegal_CustomDrawCell(object sender, DevExpress.XtraGrid.Views.Base.RowCellCustomDrawEventArgs e)
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
        private void repositoryItemDateEdit1_Closed(object sender, DevExpress.XtraEditors.Controls.ClosedEventArgs e)
        {
        }

        private void repositoryItemDateEdit2_Closed(object sender, DevExpress.XtraEditors.Controls.ClosedEventArgs e)
        {

        }

        private void repositoryItemDateEdit2_BeforePopup(object sender, EventArgs e)
        {

        }

        private DateEdit CreateDDFDateEdit()
        {
            var de = new DateEdit();

            //var binding = new Binding("EditValue", bs, bsField, true);
            //de.DataBindings.Add(binding);

            de.EditValue = null;
            de.Properties.Buttons.Clear();
            de.Properties.CalendarTimeProperties.Buttons.Clear();
            de.Properties.Buttons.AddRange(new[] { new EditorButton(ButtonPredefines.Combo) });
            de.Properties.CalendarTimeEditing = DefaultBoolean.True;
            de.Properties.CalendarTimeProperties.Buttons.AddRange(new[] { new EditorButton() });
            de.Properties.CalendarTimeProperties.DisplayFormat.FormatString = "HH:mm:ss";
            de.Properties.CalendarTimeProperties.DisplayFormat.FormatType = FormatType.DateTime;
            de.Properties.CalendarTimeProperties.EditFormat.FormatString = "HH:mm:ss";
            de.Properties.CalendarTimeProperties.EditFormat.FormatType = FormatType.DateTime;
            de.Properties.CalendarTimeProperties.Mask.EditMask = "HH:mm:ss";
            de.Properties.CalendarView = CalendarView.Vista;
            //if (dateAndTime == "g")
            //{
            //    de.Properties.DisplayFormat.FormatString = "G";
            //    de.Properties.DisplayFormat.FormatType = FormatType.DateTime;
            //    de.Properties.EditFormat.FormatString = "G";
            //    de.Properties.EditFormat.FormatType = FormatType.DateTime;
            //    de.Properties.Mask.EditMask = "G";
            //}
            de.Properties.VistaCalendarViewStyle = VistaCalendarViewStyle.MonthView;
            de.Properties.VistaDisplayMode = DefaultBoolean.True;
            de.Location = new Point(108, 3);
            //de.Name = ctrlUniqueName;
            //de.StyleController = layoutControl;
            //de.MenuManager = BarManager;
            //de.Validated += BaseEdit_Validated;
            //de.EditValueChanged += BaseEdit_EditValueChanged;
            de.QueryPopUp += BaseEdit_QueryPopUp;
            de.CausesValidation = true;

            return de;
        }

        private void BaseEdit_QueryPopUp(object sender, CancelEventArgs e)
        {
            var edit = sender as DateEdit;
            if (edit == null) return;
            if (edit.EditValue == null || edit.EditValue is DBNull)
                edit.EditValue = DateTime.Now;
        }
        /****************************************************************************************/
        private void repositoryItemCheckEdit12_CheckedChanged(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgvDependent.DataSource;
            DataRow dr = gridMainDep.GetFocusedDataRow();
            string setting = dr["hpb"].ObjToString();
            if (setting == "1")
            { // Set, so take it away
                bool mod = btnSaveAll.Visible;
                string fname = dr["depFirstName"].ObjToString();
                string lname = dr["depLastName"].ObjToString();
                string mi = dr["depMI"].ObjToString();
                string cmd = "Select * from `relatives` where `contractNumber` = '" + workContract + "' AND `depFirstName` = '" + fname + "' AND `depLastName` = '" + lname + "' AND `depMI` = '" + mi + "' ";
                cmd += " AND `depRelationship` = 'HPB';";
                DataTable dx = G1.get_db_data(cmd);
                if (dx.Rows.Count > 0)
                {
                    string record = dx.Rows[0]["record"].ObjToString();
                    G1.delete_db_table("relatives", "record", record);
                }
                funModified = mod;
                btnSaveAll.Visible = mod;
            }
            else if (setting == "0")
            { // Not Set, so set it up
                bool mod = btnSaveAll.Visible;
                DataTable dx = dt.Clone();
                int rowHandle = gridMainDep.FocusedRowHandle;
                int row = gridMainDep.GetDataSourceRowIndex(rowHandle);
                G1.copy_dt_row(dt, row, dx, 0);
                dx.Rows[0]["record"] = -1;
                SaveMembers(dx, "HPB");
                funModified = true;
                btnSaveAll.Visible = true;
            }
        }
        /****************************************************************************************/
        private void repositoryItemCheckEdit7_CheckedChanged(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgvDependent.DataSource;
            DataRow dr = gridMainDep.GetFocusedDataRow();
            string setting = dr["pb"].ObjToString();
            if (setting == "1")
            { // Set, so take it away
                bool mod = btnSaveAll.Visible;
                string fname = dr["depFirstName"].ObjToString();
                string lname = dr["depLastName"].ObjToString();
                string mi = dr["depMI"].ObjToString();
                string cmd = "Select * from `relatives` where `contractNumber` = '" + workContract + "' AND `depFirstName` = '" + fname + "' AND `depLastName` = '" + lname + "' AND `depMI` = '" + mi + "' ";
                cmd += " AND `depRelationship` = 'PB';";
                DataTable dx = G1.get_db_data(cmd);
                if (dx.Rows.Count > 0)
                {
                    string record = dx.Rows[0]["record"].ObjToString();
                    G1.delete_db_table("relatives", "record", record);
                }
                funModified = mod;
                btnSaveAll.Visible = mod;
            }
            else if (setting == "0")
            { // Not Set, so set it up
                bool mod = btnSaveAll.Visible;
                DataTable dx = dt.Clone();
                int rowHandle = gridMainDep.FocusedRowHandle;
                int row = gridMainDep.GetDataSourceRowIndex(rowHandle);
                G1.copy_dt_row(dt, row, dx, 0);
                dx.Rows[0]["record"] = -1;
                SaveMembers(dx, "PB");
                funModified = true;
                btnSaveAll.Visible = true;
            }
        }
        /****************************************************************************************/
        private void dgvDependent_VisibleChanged(object sender, EventArgs e)
        {
            string fname = "";
            string lname = "";
            string mi = "";
            string relation = "";
            DataRow[] dR = null;
            DataTable dt = (DataTable)dgvDependent.DataSource;
            if (dt == null)
                return;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                dt.Rows[i]["pb"] = "0";
                dt.Rows[i]["hpb"] = "0";
            }
            string cmd = "Select * from `relatives` where `contractNumber` = '" + workContract + "' AND (`depRelationship` = 'PB' OR `depRelationship` = 'HPB');";
            DataTable dx = G1.get_db_data(cmd);
            for (int i = 0; i < dx.Rows.Count; i++)
            {
                fname = dx.Rows[i]["depFirstName"].ObjToString();
                lname = dx.Rows[i]["depLastName"].ObjToString();
                mi = dx.Rows[i]["depMI"].ObjToString();
                relation = dx.Rows[i]["depRelationship"].ObjToString();
                try
                {
                    dR = dt.Select("depFirstName='" + fname + "' AND depLastName='" + lname + "' AND depMI='" + mi + "'");
                    if (dR.Length > 0)
                    {
                        if (relation.ToUpper() == "HPB")
                            dR[0]["hpb"] = "1";
                        else if (relation.ToUpper() == "PB")
                            dR[0]["pb"] = "1";
                    }
                }
                catch (Exception ex)
                {
                }
            }
            dgvDependent.DataSource = dt;
            gridMainDep.RefreshData();
        }
        /****************************************************************************************/
        private void dgvLegal_VisibleChanged(object sender, EventArgs e)
        {
        }
        /****************************************************************************************/
        private void dgv2_VisibleChanged(object sender, EventArgs e)
        {
            MergePallBearers();
        }
        /****************************************************************************************/
        private void dgv3_VisibleChanged(object sender, EventArgs e)
        {
            MergeHPB();
        }
        /****************************************************************************************/
        private void gridMain6_RowCellStyle(object sender, DevExpress.XtraGrid.Views.Grid.RowCellStyleEventArgs e)
        {
            GridView View = sender as GridView;
            if (e.Column.FieldName.ToUpper().IndexOf("DATA") >= 0)
            {
                //DataTable dt6 = (DataTable)dgv6.DataSource;
                string str = View.GetRowCellValue(e.RowHandle, "dbfield").ObjToString();
                if (str != null)
                {
                    if (str.ToUpper().IndexOf("CLERGY") >= 0)
                        e.Appearance.BackColor = Color.Gray;
                }
            }
        }
        /****************************************************************************************/
    }
}