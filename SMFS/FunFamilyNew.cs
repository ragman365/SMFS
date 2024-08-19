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
using Tracking;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraReports.Design;
using DevExpress.XtraEditors.Controls;
using DevExpress.Utils;
using DevExpress.XtraBars;
using System.Text.RegularExpressions;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using DevExpress.XtraEditors.ViewInfo;

using System.Runtime.InteropServices;
using System.Drawing;
using DevExpress.XtraEditors.Popup;
using DevExpress.Utils.Win;
/****************************************************************************************/
namespace SMFS
{
    /****************************************************************************************/
    public partial class FunFamilyNew : DevExpress.XtraEditors.XtraForm
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
        private bool newFamilyPB = true;
        private DataTable workDt6 = null;
        private bool gameOver = false;

        /****************************************************************************************/
        EditCust editCust = null;
        /****************************************************************************************/

        public FunFamilyNew(EditCust program, string contract, bool funeral)
        {
            editCust = program;

            InitializeComponent();
            workContract = contract;
            workLegal = false;
            workFuneral = funeral;
            //workWhat = "ALL";
        }
        /****************************************************************************************/
        public FunFamilyNew(EditCust program, string contract, bool funeral, bool legal = false)
        {
            editCust = program;

            InitializeComponent();
            workContract = contract;
            workLegal = legal;
            workFuneral = funeral;
        }
        /****************************************************************************************/
        public FunFamilyNew(string contract, string what, bool funeral)
        {
            InitializeComponent();
            workContract = contract;
            workWhat = what;
            workFuneral = funeral;
        }
        /****************************************************************************************/
        public FunFamilyNew(string contract, bool funeral, bool legal = false, string what = "", string filter = "")
        {
            InitializeComponent();
            workContract = contract;
            workLegal = legal;
            workFuneral = funeral;
            workWhat = what;
            workFilter = filter;
        }
        /****************************************************************************************/
        private void FunFamilyNew_Load(object sender, EventArgs e)
        {
            btnSaveAll.Hide();
            SetupMainGrid();
            funModified = false;
            otherModified = false;

            panelClergyStuff.Hide();
            btnHold.Hide();
            //textBox1.Hide();

            LoadFamily();

            funDemo = new FuneralDemo("Clergy", "", "", "", "", "", "", "", "", "", "", "");
            funDemo.FunDemoDone += FunDemo_FunDemoDone;
            Rectangle rect = funDemo.Bounds;
            int top = rect.Y;
            int left = rect.X;
            int height = rect.Height;
            int width = rect.Width;
            top = this.Bounds.Y;
            left = this.Bounds.Width - width;
            funDemo.StartPosition = FormStartPosition.Manual;
            funDemo.SetBounds(left, top, width, height);
            funDemo.Show();
            funDemo.Hide();


            G1.SetupToolTip(pictureBox12, "Add New Member");
            G1.SetupToolTip(pictureBox11, "Remove Member");
            G1.SetupToolTip(picRowUp, "Move Current Member Up 1 Row");
            G1.SetupToolTip(picRowDown, "Move Current Member Down 1 Row");
            //addSignatureToolStripMenuItem.Enabled = false;
            //contextMenuStrip1.Enabled = false;

            gridMain6.ExpandAllGroups();
            gridMain6.RefreshEditor(true);
            gridMain6.RefreshData();
            dgv6.Refresh();
            gridMain6.Focus();
            dgv6.Focus();
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
        public DataTable saveMembersDt = null;
        public bool preprocessDone = false;

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
            if (G1.get_column_number(dt, "pb") < 0)
                dt.Columns.Add("pb");
            if (G1.get_column_number(dt, "hpb") < 0)
                dt.Columns.Add("hpb");
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                dt.Rows[i]["pb"] = "0";
                dt.Rows[i]["hpb"] = "0";
            }

            addSignatureToolStripMenuItem.Enabled = true;
            contextMenuStrip1.Enabled = true;

            if (workWhat == "ALL")
            {
                LoadFamilyMembers(dt);
                if (saveMembersDt == null)
                {
                    saveMembersDt = editCust.masterTable;
                    //if (editCust != null)
                    //    editCust.masterTable = dt;
                }
                else if (editCust != null)
                    saveMembersDt = editCust.masterTable;
                LoadPallBearers(saveMembersDt);
                LoadHonoraryPallBearers(saveMembersDt);
                LoadClergy(dt);
                LoadMusicians(dt);
                LoadDisclosures(dt);
                LoadOtherData();
            }
            else
            {
                if (!workLegal)
                {
                    LoadFamilyMembers(dt);
                    if (preprocessDone)
                    {
                        saveMembersDt = dt;
                        masterDt = dt;
                        if (editCust != null)
                            editCust.masterTable = dt;
                    }
                    else
                    {
                        if (saveMembersDt == null)
                            saveMembersDt = editCust.FireEventGetMaster();
                        else if (editCust != null)
                            saveMembersDt = editCust.masterTable;
                    }
                    LoadPallBearers(saveMembersDt);
                    LoadHonoraryPallBearers(saveMembersDt);
                    LoadClergy(dt);
                    LoadMusicians(dt);
                    LoadDisclosures(dt);
                    LoadOtherData();
                }
                else
                {
                    dgvDependent.Visible = false;
                    dgvLegal.Visible = false;

                    LoadFamilyMembers(dt);

                    if (editCust != null)
                    {
                        if (editCust.masterTable != null)
                            saveMembersDt = editCust.masterTable;
                    }

                    if (saveMembersDt == null && editCust != null)
                    {
                        if (editCust.masterTable != null)
                            saveMembersDt = editCust.masterTable;
                        else
                            saveMembersDt = dt;
                    }

                    LoadPallBearers(saveMembersDt);
                    LoadHonoraryPallBearers(saveMembersDt);
                    LoadClergy(dt);
                    LoadMusicians(dt);
                    LoadDisclosures(dt);
                    LoadOtherData();
                }
            }
            masterDt = dt;
        }
        /***************************************************************************************/
        private bool PreProcessFamily(DataTable famDt, DataTable masterDt, bool saving = false)
        {
            bool modified = false;
            if (preprocessDone)
                return modified;

            if (masterDt == null)
                return modified;

            if (G1.get_column_number(masterDt, "pb") < 0)
                masterDt.Columns.Add("pb");
            if (G1.get_column_number(masterDt, "hpb") < 0)
                masterDt.Columns.Add("hpb");

            if (G1.get_column_number(famDt, "pb") < 0)
                famDt.Columns.Add("pb");
            if (G1.get_column_number(famDt, "hpb") < 0)
                famDt.Columns.Add("hpb");

            //for ( int i=0; i<masterDt.Rows.Count; i++)
            //{
            //    masterDt.Rows[i]["pb"] = "0";
            //    masterDt.Rows[i]["hpb"] = "0";
            //}

            DataTable relationDt = famDt;
            if (relationDt == null)
                return modified;

            string pbCheck = "";
            string hpbCheck = "";

            string checkRecord = "";
            string mod = "";

            DataRow dR1 = null;
            DataRow dR2 = null;

            DataRow[] dR = null;
            DataRow[] dRows = masterDt.Select("depRelationship='PB'");
            if (dRows.Length > 0)
            {
                DataTable testDt = dRows.CopyToDataTable();
                string fname = "";
                string lname = "";
                string mi = "";
                string record = "";
                string isChecked = "";
                string relation = "";
                int order = 0;
                pbCheck = "";
                for (int i = 0; i < dRows.Length; i++) // DataRows of Pall Bearers
                {
                    relation = dRows[i]["depRelationship"].ObjToString().ToUpper();
                    if (relation == "DISCLOSURES")
                        continue;
                    record = dRows[i]["record"].ObjToString();
                    mod = dRows[i]["mod"].ObjToString();
                    checkRecord = dRows[i]["checkRecord"].ObjToString();
                    if (!String.IsNullOrWhiteSpace(checkRecord)) // Already Tied to a Family Member;
                    {
                        dR = masterDt.Select("record='" + checkRecord + "'");
                        if (dR.Length > 0)
                        {
                            if (saving && mod == "D")
                            {
                                dR[0]["pb"] = "0";
                                dR[0]["hpb"] = "0";
                            }
                            else
                            {
                                dR[0]["pb"] = "1";
                                dR[0]["hpb"] = "0";
                            }
                        }
                        continue;
                    }
                    fname = dRows[i]["depFirstName"].ObjToString();
                    lname = dRows[i]["depLastName"].ObjToString();
                    mi = dRows[i]["depMI"].ObjToString();
                    dR = masterDt.Select("depFirstName='" + fname + "' AND depLastName='" + lname + "' AND depMI='" + mi + "'"); // List of Family with same name
                    if (dR.Length > 1) //Got Someone who may be a Pall Bearer and Family Member
                    {
                        for (int j = 0; j < dR.Length; j++)
                        {
                            if (dR[j]["depRelationship"].ObjToString().ToUpper() != "PB")
                                dR1 = dR[j];

                        }
                        for (int j = 0; j < dR.Length; j++)
                        {
                            if (dR[j]["depRelationship"].ObjToString().ToUpper() == "PB")
                                dR2 = dR[j];
                        }
                        if (dR1 != null && dR2 != null)
                        {
                            checkRecord = dR1["record"].ObjToString();
                            record = dR2["record"].ObjToString();
                            order = dR2["PalOrder"].ObjToInt32();

                            G1.copy_dr_row(dR1, dR2);

                            dR2["record"] = record;
                            dR2["PalOrder"] = order;
                            dR2["checkRecord"] = checkRecord;
                            dR2["depRelationship"] = "PB";
                            dR1["pb"] = "1";
                            modified = true;
                        }
                    }
                }
            }
            dRows = masterDt.Select("depRelationship='HPB'");
            if (dRows.Length > 0)
            {
                DataTable testDt = dRows.CopyToDataTable();
                string fname = "";
                string lname = "";
                string mi = "";
                string record = "";
                string isChecked = "";
                string relation = "";
                hpbCheck = "";
                for (int i = 0; i < dRows.Length; i++)
                {
                    relation = dRows[i]["depRelationship"].ObjToString().ToUpper();
                    if (relation == "DISCLOSURES")
                        continue;
                    record = dRows[i]["record"].ObjToString();
                    mod = dRows[i]["mod"].ObjToString();
                    checkRecord = dRows[i]["checkRecord"].ObjToString();
                    if (!String.IsNullOrWhiteSpace(checkRecord)) // Already Tied to a Family Member;
                    {
                        dR = masterDt.Select("record='" + checkRecord + "'");
                        if (dR.Length > 0)
                        {
                            if (saving && mod == "D")
                            {
                                dR[0]["pb"] = "0";
                                dR[0]["hpb"] = "0";
                            }
                            else
                            {
                                dR[0]["pb"] = "0";
                                dR[0]["hpb"] = "1";
                            }
                        }
                        continue;
                    }
                    fname = dRows[i]["depFirstName"].ObjToString();
                    lname = dRows[i]["depLastName"].ObjToString();
                    mi = dRows[i]["depMI"].ObjToString();
                    DataTable tDt = GetFamily(masterDt, fname, lname, mi);
                    dR = masterDt.Select("depFirstName='" + fname + "' AND depLastName='" + lname + "' AND depMI='" + mi + "'");
                    if (dR.Length > 1)
                    {
                        for (int j = 0; j < dR.Length; j++)
                        {
                            if (dR[j]["depRelationship"].ObjToString().ToUpper() != "HPB")
                                dR1 = dR[j];

                        }
                        for (int j = 0; j < dR.Length; j++)
                        {
                            if (dR[j]["depRelationship"].ObjToString().ToUpper() == "HPB")
                                dR2 = dR[j];
                        }
                        checkRecord = dR1["record"].ObjToString();
                        record = dR2["record"].ObjToString();

                        G1.copy_dr_row(dR1, dR2);

                        dR2["record"] = record;
                        dR2["checkRecord"] = checkRecord;
                        dR2["depRelationship"] = "HPB";
                        dR1["hpb"] = "1";
                        modified = true;
                    }
                }
            }
            preprocessDone = true;
            if (modified)
            {
                if (editCust != null)
                    editCust.masterTable = masterDt;
            }
            return modified;
        }
        /***************************************************************************************/
        private DataTable GetFamily(DataTable dx, string fName, string lName, string mi)
        {
            DataTable dt = dx.Clone();
            string fn = "";
            string ln = "";
            string mn = "";
            for (int i = 0; i < dx.Rows.Count; i++)
            {
                fn = dx.Rows[i]["depFirstName"].ObjToString().Trim();
                ln = dx.Rows[i]["depLastName"].ObjToString().Trim();
                mn = dx.Rows[i]["depMI"].ObjToString().Trim();
                if (fn == fName && ln == lName && mi == mn)
                    G1.copy_dt_row(dx, i, dt, dt.Rows.Count);
            }
            return dt;
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

            PreProcessFamily(famDt, dt);

            SetupPB(famDt, dt);
            SetupHPB(famDt, dt);

            LoadSignatures(famDt);

            G1.NumberDataTable(famDt);
            if (preprocessDone && saveMembersDt != null)
                dt = saveMembersDt;
            else if (saveMembersDt == null)
                saveMembersDt = dt;

            string phone = "";
            for ( int i=0; i< dt.Rows.Count; i++)
            {
                phone = dt.Rows[i]["phone"].ObjToString();
                if ( !String.IsNullOrWhiteSpace ( phone ))
                {
                    phone = AgentProspectReport.reformatPhone(phone, true );
                    dt.Rows[i]["phone"] = phone;
                }
            }

            G1.NumberDataTable(dt);
            dgvDependent.DataSource = dt;
            return;
            //dgvDependent.DataSource = famDt;
            //            dgvLegal.DataSource = famDt;
        }
        /***************************************************************************************/
        private DateTime lastPallBearersTime = DateTime.Now;
        private void LoadPallBearers(DataTable dt)
        {
            DataTable pallDt = dt.Clone();
            string relation = "";
            string isChecked = "";
            string checkRecord = "";
            string record = "";
            string mod = "";
            int order = 0;
            DataTable testDt = null;
            DataRow[] dRows = null;
            DataRow[] dR = null;
            string firstName = "";
            string lastName = "";
            string mi = "";
            int count = 0;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                mod = dt.Rows[i]["mod"].ObjToString();
                if (mod == "D")
                    continue;
                isChecked = dt.Rows[i]["pbCheck"].ObjToString();
                relation = dt.Rows[i]["depRelationship"].ObjToString().ToUpper();
                if (relation != "PB")
                    continue;

                checkRecord = dt.Rows[i]["checkRecord"].ObjToString();
                order = dt.Rows[i]["PalOrder"].ObjToInt32();
                record = dt.Rows[i]["record"].ObjToString();
                if (!String.IsNullOrWhiteSpace(checkRecord) && saveMembersDt != null)
                {
                    dRows = saveMembersDt.Select("record='" + checkRecord + "'");
                    if (dRows.Length > 0)
                    {
                        testDt = dRows.CopyToDataTable();
                        G1.copy_dt_row(testDt, 0, pallDt, pallDt.Rows.Count);
                        count = pallDt.Rows.Count - 1;
                        pallDt.Rows[count]["record"] = record;
                        pallDt.Rows[count]["PalOrder"] = order;
                        pallDt.Rows[count]["depRelationship"] = "PB";
                        pallDt.Rows[count]["checkRecord"] = checkRecord;
                    }
                    else
                        G1.copy_dt_row(dt, i, pallDt, pallDt.Rows.Count);
                }
                else if (relation == "PB")
                {
                    firstName = dt.Rows[i]["depFirstName"].ObjToString();
                    lastName = dt.Rows[i]["depLastName"].ObjToString();
                    mi = dt.Rows[i]["depMI"].ObjToString();

                    dR = dt.Select("depFirstName='" + firstName + "' AND depLastName='" + lastName + "' AND depMI='" + mi + "' AND pb = '1'"); // List of Family with same name
                    if (dR.Length > 0)
                    {
                        for (int j = 0; j < dR.Length; j++)
                        {
                            if (dR[j]["depRelationship"].ObjToString().ToUpper() != "PB")
                            {
                                checkRecord = dR[0]["record"].ObjToString();
                                dt.Rows[i]["checkRecord"] = checkRecord;
                                break;
                            }
                        }
                    }
                    G1.copy_dt_row(dt, i, pallDt, pallDt.Rows.Count);
                }
            }

            G1.sortTable(pallDt, "PalOrder", "ASC");

            G1.NumberDataTable(pallDt);
            dgv2.DataSource = pallDt;
            //OnFamModified();
            lastPallBearersTime = DateTime.Now;
        }
        /***************************************************************************************/
        private void LoadHonoraryPallBearers(DataTable dt)
        {
            DataTable pallDt = dt.Clone();
            string relation = "";
            string isChecked = "";
            string checkRecord = "";
            string record = "";
            string mod = "";
            int order = 0;
            DataTable testDt = null;
            DataRow[] dRows = null;
            DataRow[] dR = null;

            string firstName = "";
            string lastName = "";
            string mi = "";

            int count = 0;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                mod = dt.Rows[i]["mod"].ObjToString();
                if (mod == "D")
                    continue;
                isChecked = dt.Rows[i]["pbCheck"].ObjToString();
                relation = dt.Rows[i]["depRelationship"].ObjToString().ToUpper();
                if (relation != "HPB")
                    continue;

                checkRecord = dt.Rows[i]["checkRecord"].ObjToString();
                record = dt.Rows[i]["record"].ObjToString();
                if (!String.IsNullOrWhiteSpace(checkRecord) && saveMembersDt != null)
                {
                    dRows = saveMembersDt.Select("record='" + checkRecord + "'");
                    if (dRows.Length > 0)
                    {
                        testDt = dRows.CopyToDataTable();
                        G1.copy_dt_row(testDt, 0, pallDt, pallDt.Rows.Count);
                        count = pallDt.Rows.Count - 1;
                        pallDt.Rows[count]["record"] = record;
                        pallDt.Rows[count]["depRelationship"] = "HPB";
                        pallDt.Rows[count]["checkRecord"] = checkRecord;
                    }
                    else
                        G1.copy_dt_row(dt, i, pallDt, pallDt.Rows.Count);
                }
                else if (relation == "HPB")
                {
                    firstName = dt.Rows[i]["depFirstName"].ObjToString();
                    lastName = dt.Rows[i]["depLastName"].ObjToString();
                    mi = dt.Rows[i]["depMI"].ObjToString();

                    dR = dt.Select("depFirstName='" + firstName + "' AND depLastName='" + lastName + "' AND depMI='" + mi + "' AND hpb = '1'"); // List of Family with same name
                    if (dR.Length > 0)
                    {
                        for (int j = 0; j < dR.Length; j++)
                        {
                            if (dR[j]["depRelationship"].ObjToString().ToUpper() != "HPB")
                            {
                                checkRecord = dR[0]["record"].ObjToString();
                                dt.Rows[i]["checkRecord"] = checkRecord;
                                break;
                            }
                        }
                    }
                    G1.copy_dt_row(dt, i, pallDt, pallDt.Rows.Count);
                }
            }

            G1.sortTable(pallDt, "Order", "ASC");

            G1.NumberDataTable(pallDt);
            dgv3.DataSource = pallDt;
            //OnFamModified();
            lastPallBearersTime = DateTime.Now;
        }
        /***************************************************************************************/
        private void LoadHonoraryPallBearersx(DataTable dt)
        {
            DataTable pallDt = dt.Clone();
            string relation = "";
            string isChecked = "";
            string checkRecord = "";
            string record = "";
            string mod = "";
            int order = 0;
            DataTable testDt = null;
            DataRow[] dRows = null;
            int count = 0;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                mod = dt.Rows[i]["mod"].ObjToString();
                if (mod == "D")
                    continue;
                isChecked = dt.Rows[i]["hpbCheck"].ObjToString();
                relation = dt.Rows[i]["depRelationship"].ObjToString().ToUpper();
                if (relation != "HPB")
                    continue;

                checkRecord = dt.Rows[i]["checkRecord"].ObjToString();
                //order = dt.Rows[i]["PalOrder"].ObjToInt32();
                record = dt.Rows[i]["record"].ObjToString();
                if (!String.IsNullOrWhiteSpace(checkRecord))
                {
                    dRows = saveMembersDt.Select("record='" + checkRecord + "'");
                    if (dRows.Length > 0)
                    {
                        testDt = dRows.CopyToDataTable();
                        G1.copy_dt_row(testDt, 0, pallDt, pallDt.Rows.Count);
                        count = pallDt.Rows.Count - 1;
                        pallDt.Rows[count]["record"] = record;
                        pallDt.Rows[count]["PalOrder"] = order;
                        //pallDt.Rows[count]["depRelationship"] = "HPB";
                        pallDt.Rows[count]["checkRecord"] = checkRecord;
                    }
                    else
                        G1.copy_dt_row(dt, i, pallDt, pallDt.Rows.Count);
                }
                else if (relation == "HPB")
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
            gridMain4.Columns["depFirstName"].Visible = true;
            gridMain4.Columns["depLastName"].Visible = true;
            gridMain4.Columns["depMI"].Visible = true;
            gridMain4.Columns["depPrefix"].Visible = true;
            gridMain4.Columns["depSuffix"].Visible = true;
            gridMain4.Columns["fullName"].Visible = false;
            gridMain4.Columns["fullName"].OptionsColumn.AllowEdit = false;

            LoadClergyDropDown();

            pallDt = (DataTable)dgv4.DataSource;
        }
        /***************************************************************************************/
        private DataTable clergyDt = null;
        private void LoadClergyDropDown()
        {
            if (trackingDt == null)
            {
                trackingDt = G1.get_db_data("Select * from `tracking`;");
                trackDt = G1.get_db_data("Select * from `track`;");
            }

            string dbField = "SRVClergy";
            DataRow[] dR = trackingDt.Select("tracking='" + dbField + "'");
            if (dR.Length <= 0)
            {
                gridMain4.Columns["depLastName"].ColumnEdit = null;
                gridMain4.Columns["depFirstName"].ColumnEdit = null;
                return;
            }

            string locations = findLocationAssociation();

            if (locations.IndexOf(" OR ") > 0)
            {
                try
                {
                    dR = trackDt.Select("tracking='" + dbField + "' AND (" + locations + ")");
                }
                catch (Exception ex)
                {
                }
            }
            else
                dR = trackDt.Select("tracking='" + dbField + "' AND location='" + EditCust.activeFuneralHomeName + "'");


            string name = "";
            string prefix = "";
            string firstName = "";
            string miName = "";
            string lastName = "";
            string suffix = "";

            DataView tempview;

            DataTable ddR = null;
            if (dR.Length > 0)
            {

                ddR = dR.CopyToDataTable();

                try
                {
                    for (int i = 0; i < ddR.Rows.Count; i++)
                    {
                        name = ddR.Rows[i]["answer"].ObjToString();
                        if (String.IsNullOrWhiteSpace(name))
                            continue;
                        firstName = ddR.Rows[i]["depFirstName"].ObjToString().Trim();
                        lastName = ddR.Rows[i]["depLastName"].ObjToString().Trim();
                        if (String.IsNullOrWhiteSpace(lastName))
                        {
                            G1.ParseOutName(name, ref prefix, ref firstName, ref lastName, ref miName, ref suffix);
                            name = firstName + " " + lastName;
                            ddR.Rows[i]["answer"] = name.Trim();
                            ddR.Rows[i]["depPrefix"] = prefix.Trim();
                            ddR.Rows[i]["depFirstName"] = firstName.Trim();
                            ddR.Rows[i]["depLastName"] = lastName.Trim();
                            ddR.Rows[i]["depMI"] = miName.Trim();
                            ddR.Rows[i]["depSuffix"] = suffix.Trim();
                        }
                        else
                        {
                            name = firstName + " " + lastName;
                            ddR.Rows[i]["answer"] = name;
                        }
                    }

                    tempview = ddR.DefaultView;
                    tempview.Sort = "answer asc";
                    ddR = tempview.ToTable();

                    ddR = RemoveDuplicates(ddR, "depLastName", "depFirstName");
                    //ddR = RemoveDuplicates(ddR, "answer");

                }
                catch (Exception ex)
                {
                }
            }
            ciLookup.Items.Clear();
            repositoryItemComboBox21.Items.Clear();
            repositoryItemComboBox22.Items.Clear();
            if (ddR == null)
                return;
            if (ddR.Rows.Count <= 0)
                return;

            clergyDt = ddR.Copy();

            string str = "";

            for (int i = 0; i < ddR.Rows.Count; i++) // Load FirstName LastName DropDown
            {
                str = ddR.Rows[i]["answer"].ObjToString().Trim();
                if (!String.IsNullOrWhiteSpace(str))
                {
                    ciLookup.Items.Add( str );
                    repositoryItemComboBox22.Items.Add( str );
                }
            }

            tempview = ddR.DefaultView;
            tempview.Sort = "depLastName asc,depFirstName asc";
            ddR = tempview.ToTable();

            for (int i = 0; i < ddR.Rows.Count; i++) // Load LastName, FirstName DropDown
            {
                lastName = ddR.Rows[i]["depLastName"].ObjToString().Trim();
                firstName = ddR.Rows[i]["depFirstName"].ObjToString().Trim();
                if (!String.IsNullOrWhiteSpace(lastName) && !String.IsNullOrWhiteSpace(firstName))
                {
                    name = lastName + ", " + firstName;
                    repositoryItemComboBox21.Items.Add(name);
                }
            }

            try
            { // Load All Other Clergy Members
                dR = trackDt.Select("tracking='" + dbField + "'");
                if (dR.Length <= 0)
                    return;

                string record = "";
                DataTable tempDt = dR.CopyToDataTable();
                DataTable temp2Dt = tempDt.Clone();
                for (int i = 0; i < tempDt.Rows.Count; i++)
                {
                    record = tempDt.Rows[i]["record"].ObjToString();
                    dR = clergyDt.Select("record='" + record + "'");
                    if (dR.Length <= 0)
                    {
                        G1.copy_dt_row(tempDt, i, temp2Dt, temp2Dt.Rows.Count);
                        G1.copy_dt_row(tempDt, i, clergyDt, clergyDt.Rows.Count);
                    }
                }

                temp2Dt = RemoveDuplicates(temp2Dt, "depLastName", "depFirstName");
                //temp2Dt = RemoveDuplicates(temp2Dt, "answer");

                tempview = temp2Dt.DefaultView;
                tempview.Sort = "depLastName asc,depFirstName asc";
                temp2Dt = tempview.ToTable();

                for (int i = 0; i < temp2Dt.Rows.Count; i++)
                {
                    name = temp2Dt.Rows[i]["answer"].ObjToString();
                    lastName = temp2Dt.Rows[i]["depLastName"].ObjToString().Trim();
                    firstName = temp2Dt.Rows[i]["depFirstName"].ObjToString().Trim();
                    if (String.IsNullOrWhiteSpace(lastName))
                        G1.ParseOutName(name, ref prefix, ref firstName, ref lastName, ref miName, ref suffix);

                    if (!String.IsNullOrWhiteSpace(lastName) && !String.IsNullOrWhiteSpace(firstName))
                    {
                        name = lastName + ", " + firstName;
                        repositoryItemComboBox21.Items.Add(name);
                    }
                }

                tempview = temp2Dt.DefaultView;
                tempview.Sort = "answer asc";
                temp2Dt = tempview.ToTable();

                for (int i = 0; i < temp2Dt.Rows.Count; i++)
                {
                    name = temp2Dt.Rows[i]["answer"].ObjToString();
                    lastName = temp2Dt.Rows[i]["depLastName"].ObjToString().Trim();
                    firstName = temp2Dt.Rows[i]["depFirstName"].ObjToString().Trim();
                    if (String.IsNullOrWhiteSpace(lastName))
                        G1.ParseOutName(name, ref prefix, ref firstName, ref lastName, ref miName, ref suffix);
                    if (!String.IsNullOrWhiteSpace(lastName) && !String.IsNullOrWhiteSpace(firstName))
                    {
                        name = firstName + " " + lastName;
                        repositoryItemComboBox22.Items.Add(name);
                    }
                }
            }
            catch (Exception ex)
            {
            }
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
            dR["depRelationship"] = "DISCLOSURES";
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
                    //SaveOtherData(workContract, dt, workFuneral);
                    T1.SaveOtherData(workContract, dt, workFuneral);
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
            //if (!workLegal || workWhat == "ALL")
            //{
            //    dt = (DataTable)dgvDependent.DataSource;

            //    SaveMembers(dt);
            //    preprocessDone = false;
            //    bool modified = PreProcessFamily(dt, dt);
            //    editCust.masterTable = dt;
            //    if (modified)
            //        SaveMembers(dt);

            //    //if (editCust != null)
            //    //{
            //    //}

            //    if (workWhat != "ALL")
            //        return;
            //}

            dt = (DataTable)dgvDependent.DataSource;
            if (dt != null)
            {
                if (dt.Rows.Count > 0)
                {
                    preprocessDone = false;
                    PreProcessFamily(dt, dt, true);
                    editCust.masterTable = dt;
                }
            }

            SaveMembers(dt, "FAM");

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
            if (dt == null)
                return;
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
            string authCremation = "";
            string deceased = "";
            string legOrder = "";
            bool created = false;

            int order = 0;
            string fieldOrder = "Order";
            if (String.IsNullOrEmpty(addRelation))
                fieldOrder = "MemOrder";
            if (addRelation.ToUpper() == "PB")
                fieldOrder = "PalOrder";

            string pbCheck = "";
            string hpbCheck = "";
            string checkRecord = "";

            DataRow[] dR = null;

            string cmd = "";
            string cRecord = "";
            DataTable cDt = null;

            bool added = false;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                added = false;

                record = dt.Rows[i]["record"].ObjToString();
                if (record == "-1")
                    record = "";

                relationship = dt.Rows[i]["depRelationship"].ObjToString();
                if (addRelation.ToUpper() == "FAM")
                {
                    if (relationship == "PB")
                        continue;
                    if (relationship == "HPB")
                        continue;
                    if (relationship == "CLERGY")
                        continue;
                    if (relationship == "MUSICIAN")
                        continue;
                    if (relationship == "DISCLOSURES")
                        continue;
                }
                else
                {
                    if (!String.IsNullOrWhiteSpace(addRelation) && !String.IsNullOrWhiteSpace(relationship))
                    {
                        if (addRelation != relationship)
                            continue;
                    }
                }

                pbCheck = dt.Rows[i]["pbCheck"].ObjToString().ToUpper();
                if (addRelation.ToUpper() == "PB" && pbCheck == "1")
                    continue;
                hpbCheck = dt.Rows[i]["hpbCheck"].ObjToString();
                if (addRelation.ToUpper() == "HPB" && hpbCheck == "1")
                    continue;

                mod = dt.Rows[i]["mod"].ObjToString();
                if (mod == "D")
                {
                    if (!String.IsNullOrWhiteSpace(record))
                        G1.delete_db_table("relatives", "record", record);
                    continue;
                }


                firstName = dt.Rows[i]["depFirstName"].ObjToString();
                lastName = dt.Rows[i]["depLastName"].ObjToString();
                mi = dt.Rows[i]["depMI"].ObjToString();
                suffix = dt.Rows[i]["depSuffix"].ObjToString();
                prefix = dt.Rows[i]["depPrefix"].ObjToString();
                relationship = dt.Rows[i]["depRelationship"].ObjToString();
                fullName = "";
                if (relationship.ToUpper() == "CLERGY")
                {
                    fullName = BuildFullName(prefix, firstName, mi, lastName, suffix);
                    //fullName = dt.Rows[i]["fullName"].ObjToString();
                    //if (!String.IsNullOrWhiteSpace(fullName))
                    //    G1.ParseOutName(fullName, ref prefix, ref firstName, ref lastName, ref mi, ref suffix);
                }
                if (String.IsNullOrWhiteSpace(record) || record == "-1")
                {
                    if (String.IsNullOrWhiteSpace(lastName) && String.IsNullOrWhiteSpace(firstName) && String.IsNullOrWhiteSpace(mi))
                        continue;
                }

                if (String.IsNullOrWhiteSpace(record))
                {
                    record = G1.create_record("relatives", "depFirstName", "-1");
                    added = true;
                }
                if (G1.BadRecord("relatives", record))
                    return;

                dt.Rows[i]["record"] = record;

                firstName = dt.Rows[i]["depFirstName"].ObjToString();
                lastName = dt.Rows[i]["depLastName"].ObjToString();
                mi = dt.Rows[i]["depMI"].ObjToString();
                suffix = dt.Rows[i]["depSuffix"].ObjToString();
                prefix = dt.Rows[i]["depPrefix"].ObjToString();
                dob = dt.Rows[i]["depDOB"].ObjToString();
                dod = dt.Rows[i]["depDOD"].ObjToString();
                maidenName = dt.Rows[i]["maidenName"].ObjToString();
                relationship = dt.Rows[i]["depRelationship"].ObjToString();
                checkRecord = dt.Rows[i]["checkRecord"].ObjToString();

                if (relationship.ToUpper() == "PB" && String.IsNullOrWhiteSpace(checkRecord) && added)
                {
                    dR = dt.Select("depFirstName='" + firstName + "' AND depLastName='" + lastName + "' AND depMI='" + mi + "' AND pb = '1'"); // List of Family with same name
                    if (dR.Length > 0)
                    {
                        for (int j = 0; j < dR.Length; j++)
                        {
                            if (dR[j]["depRelationship"].ObjToString().ToUpper() != "PB")
                            {
                                checkRecord = dR[0]["record"].ObjToString();
                                break;
                            }
                        }
                    }
                }
                if (relationship.ToUpper() == "HPB" && String.IsNullOrWhiteSpace(checkRecord) && added)
                {
                    dR = dt.Select("depFirstName='" + firstName + "' AND depLastName='" + lastName + "' AND depMI='" + mi + "' AND hpb = '1'"); // List of Family with same name
                    if (dR.Length > 0)
                    {
                        for (int j = 0; j < dR.Length; j++)
                        {
                            if (dR[j]["depRelationship"].ObjToString().ToUpper() != "HPB")
                            {
                                checkRecord = dR[0]["record"].ObjToString();
                                break;
                            }
                        }
                    }
                }



                if (addRelation != "FAM")
                {
                    if (!String.IsNullOrWhiteSpace(addRelation))
                        relationship = addRelation;
                }

                fullName = "";
                if (relationship.ToUpper() == "CLERGY")
                {
                    fullName = BuildFullName(prefix, firstName, mi, lastName, suffix);
                    //fullName = dt.Rows[i]["fullName"].ObjToString();
                    //if (!String.IsNullOrWhiteSpace(fullName))
                    //    G1.ParseOutName(fullName, ref prefix, ref firstName, ref lastName, ref mi, ref suffix);
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
                G1.update_db_table("relatives", "record", record, new string[] { "address", address, "city", city, "state", state, "zip", zip, "spouseFirstName", spouseFirstName, "depDOD", dod, "phone", phone, "phoneType", phoneType, "email", email, "pbCheck", pbCheck, "hpbCheck", hpbCheck, "checkRecord", checkRecord });

                nextOfKin = dt.Rows[i]["nextOfKin"].ObjToString();
                informant = dt.Rows[i]["informant"].ObjToString();
                purchaser = dt.Rows[i]["purchaser"].ObjToString();
                deceased = dt.Rows[i]["deceased"].ObjToString();
                legOrder = dt.Rows[i]["LegOrder"].ObjToString();
                authEmbalming = dt.Rows[i]["authEmbalming"].ObjToString();
                authCremation = dt.Rows[i]["authCremation"].ObjToString();

                order++;

                G1.update_db_table("relatives", "record", record, new string[] { "LegOrder", legOrder, "nextOfKin", nextOfKin, "informant", informant, "purchaser", purchaser, "deceased", deceased, "authEmbalming", authEmbalming, "authCremation", authCremation, fieldOrder, order.ToString() });
                dt.Rows[i]["record"] = record;

                if (relationship.ToUpper() == "CLERGY")
                {
                    if (mod != "Y")
                        continue;
                    created = false;
                    cmd = "Select * from `track` WHERE `tracking` = 'SRVClergy' AND `answer` = '" + fullName + "';";
                    cDt = G1.get_db_data(cmd);
                    if (cDt.Rows.Count <= 0)
                    {
                        cRecord = G1.create_record("track", "tracking", "SRVClergy");
                        if (G1.BadRecord("track", cRecord))
                            continue;
                        created = true;
                    }
                    else
                        cRecord = cDt.Rows[0]["record"].ObjToString();

                    G1.update_db_table("track", "record", cRecord, new string[] { "depFirstName", firstName, "depLastName", lastName, "depMI", mi, "depPrefix", prefix, "depSuffix", suffix, "answer", fullName });
                    G1.update_db_table("track", "record", cRecord, new string[] { "contactType", "Clergy", "address", address, "city", city, "state", state, "zip", zip, "phone", phone, "email", email });
                    if (created)
                        G1.update_db_table("track", "record", cRecord, new string[] { "location", EditCust.activeFuneralHomeName });
                }
            }

            CleanupMembers(dt);

        }
        /***********************************************************************************************/
        private void CleanupMembers(DataTable dt)
        {
            string record = "";
            string mod = "";
            for (int i = (dt.Rows.Count - 1); i >= 0; i--)
            {
                record = dt.Rows[i]["record"].ObjToString();
                mod = dt.Rows[i]["mod"].ObjToString();
                if (mod == "D")
                    dt.Rows.RemoveAt(i);
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
            string authCremation = "";
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
                authCremation = dt.Rows[i]["authCremation"].ObjToString();

                G1.update_db_table("relatives", "record", record, new string[] { "nextOfKin", nextOfKin, "informant", informant, "purchaser", purchaser, "authEmbalming", authEmbalming, "authCremation", authCremation });
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
                //if ( dgv2.Visible )
                //{
                //    dgv2.Refresh();
                //    this.ForceRefresh();
                //    OnPallModified();
                //}
            }
            GetCurrentGridView().RefreshEditor(true);
            GetCurrentGridView().RefreshData();
            this.ForceRefresh();
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
            dRow["authCremation"] = "0";

            if (whichTab != "MAIN")
            {
                if (dgv2.Visible)
                    dRow["depRelationship"] = "PB";
                else if (dgv3.Visible)
                    dRow["depRelationship"] = "HPB";
                else if (dgv4.Visible)
                    dRow["depRelationship"] = "CLERGY";
                else if (dgv5.Visible)
                    dRow["depRelationship"] = "MUSICIAN";
            }

            dt.Rows.Add(dRow);
            int row = dt.Rows.Count;
            GetCurrentDataGrid().DataSource = dt;
            GetCurrentDataGrid().Refresh();
            gridMainDep_CellValueChanged(null, null);
            //if (dgv2.Visible)
            //{
            //    dgv2.DataSource = dt;
            //    dgv2.RefreshDataSource();
            //    dgv2.Refresh();
            //    this.Refresh();
            //    editCust.Refresh();
            //}
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
        private int whichRowChanged = -1;
        private void ClergyChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            DataRow dr = gridMain4.GetFocusedDataRow();
            if (dr == null)
                return;
            dr["mod"] = "Y";
            string fName = dr["depFirstName"].ObjToString();
            funModified = true;
            btnSaveAll.Show();
            GridColumn currCol = gridMain4.FocusedColumn;
            currentColumn = currCol.FieldName;
            if (currentColumn.ToUpper() != "DEPLASTNAME" && currentColumn.ToUpper() != "DEPFIRSTNAME")
                return;
            if (1 == 1)
                return;
            DataTable dt = (DataTable)dgv4.DataSource;
            if (gridMain4.Columns["depLastName"].ColumnEdit != null && currentColumn.ToUpper() == "DEPLASTNAME")
            {
                int rowHandle = gridMain4.FocusedRowHandle;
                int row = gridMain4.GetDataSourceRowIndex(rowHandle);
                //                string what = BuildClergyName(dt, row);
                string what = dr[currentColumn].ObjToString();

                if (String.IsNullOrWhiteSpace(what))
                    return;
                string lastName = what;
                string firstName = "";
                string[] Lines = what.Split(',');
                lastName = Lines[0].Trim();
                if (Lines.Length > 1)
                    firstName = Lines[1].Trim();

                bool found = false;

                DataTable cDt = null;
                string locations = "";
                if (!String.IsNullOrWhiteSpace(EditCust.activeFuneralHomeName))
                    locations = findLocationAssociation();

                string cmd = "";
                if (!String.IsNullOrWhiteSpace(locations))
                {
                    if (locations.IndexOf(" OR ") > 0)
                    {
                        try
                        {
                            cmd = "Select * from `track` WHERE `tracking` = 'SRVClergy' AND (" + locations + ") AND ( `deplastname` LIKE '%" + lastName + "%' );";
                            cDt = G1.get_db_data(cmd);
                            if (cDt.Rows.Count <= 0)
                            {
                                cmd = "Select * from `track` WHERE `tracking` = 'SRVClergy' AND (" + locations + ") AND ( `answer` LIKE '%" + firstName + "%' AND `answer` LIKE '%" + lastName + "%' );";
                                cDt = G1.get_db_data(cmd);
                            }
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                    else
                    {
                        try
                        {
                            cmd = "Select * from `track` WHERE `tracking` = 'SRVClergy' AND (" + locations + ") AND ( `deplastname` LIKE '%" + lastName + "%' );";
                            cDt = G1.get_db_data(cmd);
                            if (cDt.Rows.Count <= 0)
                            {
                                cmd = "Select * from `track` WHERE `tracking` = 'SRVClergy' AND (" + locations + ") AND ( `answer` LIKE '%" + firstName + "%' AND `answer` LIKE '%" + lastName + "%' );";
                                cDt = G1.get_db_data(cmd);
                            }
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                }
                else
                {
                    cmd = "Select * from `track` WHERE `tracking` = 'SRVClergy' AND ( `answer` LIKE '%" + firstName + "%' AND `answer` LIKE '%" + lastName + "%' );";
                    cDt = G1.get_db_data(cmd);
                }

                if (cDt.Rows.Count > 0)
                {
                    what = cDt.Rows[0]["answer"].ObjToString();
                    string prefix = cDt.Rows[0]["depprefix"].ObjToString();
                    firstName = cDt.Rows[0]["depfirstname"].ObjToString();
                    lastName = cDt.Rows[0]["deplastname"].ObjToString();
                    string mi = cDt.Rows[0]["depmi"].ObjToString();
                    string suffix = cDt.Rows[0]["depsuffix"].ObjToString();
                    if (String.IsNullOrWhiteSpace(lastName))
                        G1.ParseOutName(what, ref prefix, ref firstName, ref lastName, ref mi, ref suffix);
                    dr["depPrefix"] = prefix;
                    dr["depFirstName"] = firstName;
                    dr["depLastName"] = lastName;
                    dr["depMI"] = mi;
                    dr["depSuffix"] = suffix;
                    dr["fullName"] = what;
                    found = true;
                }
                if (!found)
                {
                    dr["add"] = "+";
                    dr["edit"] = "E"; //RAMMA

                    whichRowChanged = gridMain4.FocusedRowHandle;

                    string title = dr["depprefix"].ObjToString();
                    //firstName = dr["depFirstName"].ObjToString();
                    //lastName = dr["depLastName"].ObjToString();
                    string middleName = dr["depMI"].ObjToString();
                    string suffix = dr["depsuffix"].ObjToString();
                    string item = G1.BuildFullName(dr);
                    string address = dr["address"].ObjToString();
                    string city = dr["city"].ObjToString();
                    string county = dr["county"].ObjToString();
                    string state = dr["state"].ObjToString();
                    string zip = dr["zip"].ObjToString();
                    string phone = dr["phone"].ObjToString();
                    string location = "";

                    cmd = "Select * from `track` WHERE `tracking` = 'SRVClergy' AND ( `depFirstName` LIKE '%" + firstName + "%' AND `depLastName` LIKE '%" + lastName + "%' );";
                    cDt = G1.get_db_data(cmd);
                    if (cDt.Rows.Count > 0)
                    {
                        dr["depPrefix"] = cDt.Rows[0]["depPrefix"].ObjToString();
                        dr["depFirstName"] = cDt.Rows[0]["depFirstName"].ObjToString();
                        dr["depLastName"] = cDt.Rows[0]["depLastName"].ObjToString();
                        dr["depMI"] = cDt.Rows[0]["depMI"].ObjToString();
                        dr["depSuffix"] = cDt.Rows[0]["depSuffix"].ObjToString();
                        dr["address"] = cDt.Rows[0]["address"].ObjToString();
                        dr["city"] = cDt.Rows[0]["city"].ObjToString();
                        dr["county"] = cDt.Rows[0]["county"].ObjToString();
                        dr["state"] = cDt.Rows[0]["state"].ObjToString();
                        dr["zip"] = cDt.Rows[0]["zip"].ObjToString();
                        dr["phone"] = cDt.Rows[0]["phone"].ObjToString();
                        dr["add"] = "";
                        dr["edit"] = "";
                        return;
                    }


                    try
                    {
                        if (funDemo.IsDisposed)
                        {
                            funDemo = new FuneralDemo("Clergy", "", "", "", "", "", "", "", "", "", "", "");
                            funDemo.FunDemoDone += FunDemo_FunDemoDone;
                            funDemo.Show();
                        }
                        funDemo.FireEventFunDemoLoad("Person", "Clergy", title, firstName, middleName, lastName, suffix, item, address, city, county, state, zip, phone, location);
                        funDemo.TopMost = true;
                        if (!funDemo.Visible)
                        {
                            funDemo.Visible = true;
                            funDemo.Refresh();
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                }
                else
                {
                    dr["add"] = "";
                    dr["edit"] = "";
                }
            }
            else if (gridMain4.Columns["depFirstName"].ColumnEdit != null && currentColumn.ToUpper() == "DEPFIRSTNAME")
            {
                int rowHandle = gridMain4.FocusedRowHandle;
                int row = gridMain4.GetDataSourceRowIndex(rowHandle);
                //                string what = BuildClergyName(dt, row);
                string what = dr[currentColumn].ObjToString();

                if (String.IsNullOrWhiteSpace(what))
                    return;
                string lastName = "";
                string firstName = what;
                string[] Lines = what.Split(' ');
                firstName = Lines[0].Trim();
                if (Lines.Length > 1)
                    lastName = Lines[1].Trim();

                bool found = false;

                DataTable cDt = null;
                string locations = "";
                if (!String.IsNullOrWhiteSpace(EditCust.activeFuneralHomeName))
                    locations = findLocationAssociation();

                string cmd = "";
                if (!String.IsNullOrWhiteSpace(locations))
                {
                    if (locations.IndexOf(" OR ") > 0)
                    {
                        try
                        {
                            cmd = "Select * from `track` WHERE `tracking` = 'SRVClergy' AND (" + locations + ") AND ( `depfirstname` LIKE '%" + firstName + "%' );";
                            cDt = G1.get_db_data(cmd);
                            if (cDt.Rows.Count <= 0)
                            {
                                cmd = "Select * from `track` WHERE `tracking` = 'SRVClergy' AND (" + locations + ") AND ( `answer` LIKE '%" + firstName + "%' AND `answer` LIKE '%" + lastName + "%' );";
                                cDt = G1.get_db_data(cmd);
                            }
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                    else
                    {
                        try
                        {
                            cmd = "Select * from `track` WHERE `tracking` = 'SRVClergy' AND (" + locations + ") AND ( `depfirstname` LIKE '%" + firstName + "%' );";
                            cDt = G1.get_db_data(cmd);
                            if (cDt.Rows.Count <= 0)
                            {
                                cmd = "Select * from `track` WHERE `tracking` = 'SRVClergy' AND (" + locations + ") AND ( `answer` LIKE '%" + firstName + "%' AND `answer` LIKE '%" + lastName + "%' );";
                                cDt = G1.get_db_data(cmd);
                            }
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                }
                else
                {
                    cmd = "Select * from `track` WHERE `tracking` = 'SRVClergy' AND ( `answer` LIKE '%" + firstName + "%' AND `answer` LIKE '%" + lastName + "%' );";
                    cDt = G1.get_db_data(cmd);
                }

                if (cDt.Rows.Count > 0)
                {
                    what = cDt.Rows[0]["answer"].ObjToString();
                    string prefix = cDt.Rows[0]["depprefix"].ObjToString();
                    firstName = cDt.Rows[0]["depfirstname"].ObjToString();
                    lastName = cDt.Rows[0]["deplastname"].ObjToString();
                    string mi = cDt.Rows[0]["depmi"].ObjToString();
                    string suffix = cDt.Rows[0]["depsuffix"].ObjToString();
                    if (String.IsNullOrWhiteSpace(lastName))
                        G1.ParseOutName(what, ref prefix, ref firstName, ref lastName, ref mi, ref suffix);
                    dr["depPrefix"] = prefix;
                    dr["depFirstName"] = firstName;
                    dr["depLastName"] = lastName;
                    dr["depMI"] = mi;
                    dr["depSuffix"] = suffix;
                    dr["fullName"] = what;
                    found = true;
                }
                if (!found)
                {
                    dr["add"] = "+";
                    dr["edit"] = "E"; //RAMMA

                    whichRowChanged = gridMain4.FocusedRowHandle;

                    string title = dr["depprefix"].ObjToString();
                    firstName = dr["depFirstName"].ObjToString();
                    lastName = dr["depLastName"].ObjToString();
                    string middleName = dr["depMI"].ObjToString();
                    string suffix = dr["depsuffix"].ObjToString();
                    string item = G1.BuildFullName(dr);
                    string address = dr["address"].ObjToString();
                    string city = dr["city"].ObjToString();
                    string county = dr["county"].ObjToString();
                    string state = dr["state"].ObjToString();
                    string zip = dr["zip"].ObjToString();
                    string phone = dr["phone"].ObjToString();
                    string location = "";

                    funDemo.FireEventFunDemoLoad("Person", "Clergy", title, firstName, middleName, lastName, suffix, item, address, city, county, state, zip, phone, location);
                    funDemo.TopMost = true;
                    if (!funDemo.Visible)
                    {
                        funDemo.Visible = true;
                        funDemo.Refresh();
                    }
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
            if (whichTab != "MAIN")
            {
                if (dgv4.Visible)
                {
                    ClergyChanged(sender, e);
                    return;
                }
            }

            DataTable dt = GetCurrentDataTable();
            if (dt == null)
                return;

            DataRow dr = GetCurrentGridView().GetFocusedDataRow();
            if (dr == null)
                return;
            int rowHandle = GetCurrentGridView().FocusedRowHandle;
            int row = GetCurrentGridView().GetFocusedDataSourceRowIndex();

            DataTable ddt = (DataTable)dgv3.DataSource;

            string checkRecord = "";
            string pb = "";
            string hpb = "";
            string record = "";
            string pallRecord = "";
            int order = 0;

            DataTable pallDt = null;
            DataRow[] dRows = null;
            DataTable testDt = null;
            DataTable test2Dt = null;
            DataRow dR1 = null;

            if (whichTab == "MAIN")
            {
                if (dgvDependent.Visible) // Maybe Fix PB DataTable if necessary
                {
                    dt.AcceptChanges();
                    saveMembersDt = dt;
                    masterDt = dt;
                    if (editCust != null)
                        editCust.masterTable = dt;

                    record = dr["record"].ObjToString();
                    pb = dr["pb"].ObjToString();
                    hpb = dr["hpb"].ObjToString();
                    if (pb == "1")
                    {
                        OnFamModified();
                    }
                    else if (hpb == "1")
                    {
                        OnFamModified();
                    }
                }
            }
            else
            {
                if (dgv2.Visible) // Pall Bearers Changed
                {
                    OnPallModified();
                    //dt.AcceptChanges();
                    //string pbCheck = dr["pbCheck"].ObjToString();
                    //if (pbCheck == "1")
                    //{
                    //    record = dr["record"].ObjToString();
                    //    if ( !String.IsNullOrWhiteSpace ( record ) && saveMembersDt != null )
                    //    {
                    //        dRows = saveMembersDt.Select("record='" + record + "'");
                    //        if ( dRows.Length > 0 )
                    //        {
                    //            dRows[0] = dr;
                    //            saveMembersDt.AcceptChanges();
                    //            dgvDependent.DataSource = saveMembersDt;
                    //            dgvDependent.Refresh();
                    //        }
                    //    }
                    //}
                }
                else if (dgv3.Visible) // Honorary Pall Bearers Changed
                {
                    OnHPallModified();
                    //dt.AcceptChanges();
                    //string hpbCheck = dr["hpbCheck"].ObjToString();
                    //if (hpbCheck == "1")
                    //{
                    //    record = dr["record"].ObjToString();
                    //    if (!String.IsNullOrWhiteSpace(record) && saveMembersDt != null)
                    //    {
                    //        dRows = saveMembersDt.Select("record='" + record + "'");
                    //        if (dRows.Length > 0)
                    //        {
                    //            dRows[0] = dr;
                    //            saveMembersDt.AcceptChanges();
                    //            dgvDependent.DataSource = saveMembersDt;
                    //            dgvDependent.Refresh();
                    //        }
                    //    }
                    //}
                }
            }
            if (e != null)
            {
                if (whichTab == "MAIN")
                {
                    if (dgvDependent != null)
                    {
                        if (dgvDependent.Visible)
                        {
                            if (e.Column.FieldName.ToUpper() == "PB")
                                return;
                        }
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
            if (whichTab == "MAIN")
            {
                if (dgvDependent.Visible)
                    currentDGV = dgvDependent;
                else if (dgvLegal.Visible)
                    currentDGV = dgvLegal;
            }
            else
            {
                if (dgv2.Visible)
                    currentDGV = dgv2;
                else if (dgv3.Visible)
                    currentDGV = dgv3;
                else if (dgv4.Visible)
                    currentDGV = dgv4;
                else if (dgv5.Visible)
                    currentDGV = dgv5;
            }
            return currentDGV;
        }
        /****************************************************************************************/
        private DataTable GetCurrentDataTable()
        {
            DataTable dt = null;
            if (whichTab == "MAIN")
            {
                if (dgvDependent.Visible)
                    dt = (DataTable)dgvDependent.DataSource;
                else if (dgvLegal.Visible)
                    dt = (DataTable)dgvLegal.DataSource;
            }
            else
            {
                if (dgv2.Visible)
                    dt = (DataTable)dgv2.DataSource;
                else if (dgv3.Visible)
                    dt = (DataTable)dgv3.DataSource;
                else if (dgv4.Visible)
                    dt = (DataTable)dgv4.DataSource;
                else if (dgv5.Visible)
                    dt = (DataTable)dgv5.DataSource;
                else if (dgv6.Visible)
                    dt = (DataTable)dgv6.DataSource;
            }
            return dt;
        }
        /****************************************************************************************/
        private DataRow GetCurrentDataRow()
        {
            DataRow dr = null;
            if (whichTab == "MAIN")
            {
                if (dgvDependent.Visible)
                    dr = gridMainDep.GetFocusedDataRow();
                else if (dgvLegal.Visible)
                    dr = gridMainLegal.GetFocusedDataRow();
            }
            else
            {
                if (dgv2.Visible)
                    dr = gridMain2.GetFocusedDataRow();
                else if (dgv3.Visible)
                    dr = gridMain3.GetFocusedDataRow();
                else if (dgv4.Visible)
                    dr = gridMain4.GetFocusedDataRow();
                else if (dgv5.Visible)
                    dr = gridMain5.GetFocusedDataRow();
                else if (dgv6.Visible)
                    dr = gridMain6.GetFocusedDataRow();
            }
            return dr;
        }
        /****************************************************************************************/
        private DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView GetCurrentGridView()
        {
            DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gv = null;
            if (whichTab == "MAIN")
            {
                if (dgvDependent.Visible)
                    gv = gridMainDep;
                else if (dgvLegal.Visible)
                    gv = gridMainLegal;
            }
            else
            {
                if (dgv2.Visible)
                    gv = gridMain2;
                else if (dgv3.Visible)
                    gv = gridMain3;
                else if (dgv4.Visible)
                    gv = gridMain4;
                else if (dgv5.Visible)
                    gv = gridMain5;
                else if (dgv6.Visible)
                    gv = gridMain6;
            }
            return gv;
        }
        /****************************************************************************************/
        private void pictureBox11_Click(object sender, EventArgs e)
        {
            DataRow dr = GetCurrentDataRow();
            if (dr == null)
                return;
            string pbCheck = dr["pbCheck"].ObjToString();
            string hpbCheck = dr["hpbCheck"].ObjToString();

            string relation = dr["depRelationship"].ObjToString().Trim();
            string firstName = dr["depFirstName"].ObjToString();
            string lastName = dr["depLastName"].ObjToString();
            string name = lastName + ", " + firstName;
            string fullName = dr["fullName"].ObjToString();
            if (whichTab != "MAIN")
            {
                if (dgv4.Visible && !String.IsNullOrWhiteSpace(fullName))
                    name = fullName;
            }
            DialogResult result = MessageBox.Show("***Question*** Are you sure you want to DELETE this Relation\n(" + name + ") ?", "Delete Relation Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No)
                return;
            DataTable dt = GetCurrentDataTable();
            if (dt == null)
                return;
            int rowHandle = GetCurrentGridView().FocusedRowHandle;
            int row = GetCurrentGridView().GetDataSourceRowIndex(rowHandle);

            if (whichTab == "MAIN")
            {
                if (dgvDependent.Visible)
                {
                    string record = dr["record"].ObjToString();
                    dr["mod"] = "D";
                    dt.Rows[row]["mod"] = "D";
                    string pb = dr["pb"].ObjToString();
                    string hpb = dr["hpb"].ObjToString();
                    if (pb == "1")
                    {
                        if (editCust != null)
                        {
                            DataRow[] dRows = editCust.masterTable.Select("checkRecord='" + record + "'");
                            if (dRows.Length > 0)
                                dRows[0]["mod"] = "D";
                            editCust.masterTable = (DataTable)dgvDependent.DataSource;
                        }
                        OnPallModified();
                    }
                    else if (hpb == "1")
                    {
                        if (editCust != null)
                        {
                            DataRow[] dRows = editCust.masterTable.Select("checkRecord='" + record + "'");
                            if (dRows.Length > 0)
                                dRows[0]["mod"] = "D";
                            editCust.masterTable = (DataTable)dgvDependent.DataSource;
                        }
                        OnHPallModified();
                    }
                    else
                    {
                        if (editCust != null)
                            editCust.masterTable = (DataTable)dgvDependent.DataSource;
                    }
                    funModified = true;
                    btnSaveAll.Show();
                    return;
                }
            }
            else
            {
                if (dgv2.Visible)
                {
                    string record = dr["record"].ObjToString();
                    string checkRecord = dr["checkRecord"].ObjToString();
                    dr["mod"] = "D";
                    dt.Rows[row]["mod"] = "D";
                    funModified = true;
                    btnSaveAll.Show();
                    if (editCust == null)
                        return;
                    if (!String.IsNullOrWhiteSpace(checkRecord))
                    {
                        editCust.masterTable = (DataTable)dgvDependent.DataSource;
                        DataRow[] dRows = editCust.masterTable.Select("checkRecord='" + checkRecord + "'");
                        if (dRows.Length > 0)
                        {
                            dRows[0]["record"] = record;
                            dRows[0]["mod"] = "D";
                            dgvDependent.DataSource = editCust.masterTable;
                        }
                        dRows = editCust.masterTable.Select("record='" + checkRecord + "'");
                        if (dRows.Length > 0)
                            dRows[0]["pb"] = "0";
                    }
                    return;
                }
                else if (dgv3.Visible)
                {
                    string record = dr["record"].ObjToString();
                    string checkRecord = dr["checkRecord"].ObjToString();
                    dr["mod"] = "D";
                    dt.Rows[row]["mod"] = "D";
                    funModified = true;
                    btnSaveAll.Show();
                    if (editCust == null)
                        return;
                    if (!String.IsNullOrWhiteSpace(checkRecord))
                    {
                        editCust.masterTable = (DataTable)dgvDependent.DataSource;
                        DataRow[] dRows = editCust.masterTable.Select("checkRecord='" + checkRecord + "'");
                        if (dRows.Length > 0)
                        {
                            dRows[0]["record"] = record;
                            dRows[0]["mod"] = "D";
                            dgvDependent.DataSource = editCust.masterTable;
                        }
                        dRows = editCust.masterTable.Select("record='" + checkRecord + "'");
                        if (dRows.Length > 0)
                            dRows[0]["hpb"] = "0";
                    }
                    return;
                }
            }

            dr["Mod"] = "D";
            dt.Rows[row]["mod"] = "D";
            funModified = true;
            btnSaveAll.Show();

            GetCurrentGridView().RefreshData();
            GetCurrentGridView().RefreshEditor(true);
        }
        /****************************************************************************************/
        private void gridMainDep_CustomRowFilter(object sender, DevExpress.XtraGrid.Views.Base.RowFilterEventArgs e)
        {
            int row = e.ListSourceRow;
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
                string relation = dt.Rows[row]["depRelationship"].ObjToString().ToUpper();
                if (whichTab == "MAIN")
                {
                    if (String.IsNullOrWhiteSpace(relation))
                    {
                    }
                    if (newFamilyPB && dgvDependent.Visible)
                    {
                        bool filter = false;
                        if (relation == "PB")
                            filter = true;
                        else if (relation == "HPB")
                            filter = true;
                        else if (relation == "CLERGY")
                            filter = true;
                        else if (relation == "MUSICIAN")
                            filter = true;
                        else if (relation == "DISCLOSURES")
                            filter = true;
                        if (filter)
                        {
                            e.Visible = false;
                            e.Handled = true;
                            return;
                        }
                    }
                    return;
                }
                else
                {
                    if (newFamilyPB && dgv2.Visible)
                    {
                        string pbCheck = dt.Rows[row]["pbCheck"].ObjToString();
                        if (pbCheck == "1")
                            return;
                        if (relation == "PB")
                            return;
                        e.Visible = false;
                        e.Handled = true;
                    }
                    else if (dgvLegal.Visible)
                    {
                        if (relation == "DISCLOSURES")
                        {
                            e.Visible = false;
                            e.Handled = true;
                            return;
                        }
                    }
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

                text = dt.Rows[i]["authCremation"].ObjToString();
                if (!String.IsNullOrWhiteSpace(text))
                    dt.Rows[i]["authCremation"] = text;
                else
                    dt.Rows[i]["authCremation"] = "0";

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
            DevExpress.XtraEditors.CheckEdit checkBox = (DevExpress.XtraEditors.CheckEdit)sender;

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

                string dataZ = dr[currentColumn].ObjToString();

                string data = dt.Rows[row][currentColumn].ObjToString();

                if (checkBox.Checked)
                    dt.Rows[row][currentColumn] = "0";
                else
                    dt.Rows[row][currentColumn] = "1";

                string record = dt.Rows[row]["record"].ObjToString();

                DataRow[] dRow = dx.Select("record='" + record + "'");

                if (dRow.Length > 0)
                {
                    dRow[0]["mod"] = "Y";
                    dRow[0][currentColumn] = dt.Rows[row][currentColumn].ObjToString();
                    dgvDependent.DataSource = dx;
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
            this.Cursor = Cursors.WaitCursor;
            SaveRelatives();
            funModified = false;
            if (otherModified)
            {
                DateTime srvDate = DateTime.MinValue;
                DataTable dt = (DataTable)dgv6.DataSource;
                DataRow[] dRows = dt.Select("dbfield='SRVDATE'");
                if (dRows.Length > 0)
                    srvDate = dRows[0]["data"].ObjToDateTime();
                //FunFamily.SaveOtherData(workContract, dt, workFuneral);
                T1.SaveOtherData(workContract, dt, workFuneral);
                //SaveOtherData(workContract, dt, workFuneral);

                OnSomethingChanged("SRVDATE");

                otherModified = false;
            }
            btnSaveAll.Hide();
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private DataTable trackingDt = null;
        private DataTable trackDt = null;
        RepositoryItemComboBox ciLookup = new RepositoryItemComboBox();
        RepositoryItemComboBox ciLookup6 = new RepositoryItemComboBox();
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
            ciLookup6.SelectedIndexChanged += CiLookup6_SelectedIndexChanged;
            //ciLookup6.KeyPress += CiLookup6_KeyPress;
            ciLookup6.Popup += CiLookup6_Popup;

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
            dx.Columns.Add("tracking");
            dx.Columns.Add("dropOnly");
            dx.Columns.Add("addContact");
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
                    {
                        dx.Rows[i]["help"] = "Tracking";
                        dx.Rows[i]["tracking"] = "T";
                        dx.Rows[i]["dropOnly"] = dR[0]["dropOnly"].ObjToString();
                        dx.Rows[i]["addContact"] = dR[0]["addContact"].ObjToString();
                    }
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
            gridMain6.RefreshEditor(true); // RAMMA ZAMMA
            gridMain6.RefreshData();
            dgv6.Refresh();
            dgv6.Focus();
            gridMain6.Focus();
        }
        /***************************************************************************************/
        private void CiLookup6_Popup(object sender, EventArgs e)
        {
            popupForm6 = (sender as IPopupControl).PopupWindow as PopupListBoxForm;
            popupForm6.ListBox.MouseMove += ListBox6_MouseMove;
            popupForm6.ListBox.MouseDown += ListBox6_MouseDown;
            popupForm6.ListBox.SelectedValueChanged += ListBox6_SelectedValueChanged;
        }
        /****************************************************************************************/
        //private int lastIndex = -1;
        //private int whichRowChanged = -1;
        //private FuneralDemo funDemo = null;
        /****************************************************************************************/
        private void ListBox6_SelectedValueChanged(object sender, EventArgs e)
        {
            if (1 == 1) //ramma zamma
                return;
            DataRow dr = gridMain6.GetFocusedDataRow();
            string item = dr["data"].ObjToString();
            item = popupForm6.ListBox.SelectedValue.ObjToString();
            //dr["data"] = item;
            //gridMain6.RefreshData();
            //gridMain6.RefreshEditor(true);
            textBox1.Text = item;
            textBox1.Refresh();
            int index = popupForm6.ListBox.SelectedIndex;
            if (index == lastIndex)
                return;
            if (1 == 1)
                return;

            whichRowChanged = gridMain6.FocusedRowHandle;

            string columnName = gridMain6.FocusedColumn.FieldName.ToUpper();


            lastIndex = index;

            string answer = item;
            string address = "";
            string city = "";
            string county = "";
            string state = "";
            string zip = "";
            string phone = "";
            string location = "";

            try
            {
                DataRow[] dRows = null;
                dRows = itemDt.Select("answer='" + item + "'");
                if (dRows.Length > 0)
                {
                    location = dRows[0]["location"].ObjToString();
                    address = dRows[0]["address"].ObjToString();
                    city = dRows[0]["city"].ObjToString();
                    county = dRows[0]["county"].ObjToString();
                    state = dRows[0]["state"].ObjToString();
                    zip = dRows[0]["zip"].ObjToString();
                    phone = dRows[0]["phone"].ObjToString();
                }
                else
                {
                }
            }
            catch (Exception ex)
            {
            }
            if (funDemo == null)
            {
                funDemo = new FuneralDemo("Place", "", "", "", "", "", "", "", "", "", "", "");
                funDemo.FunDemoDone += FunDemo_FunDemoDone6;
                Rectangle rect = funDemo.Bounds;
                int top = rect.Y;
                int left = rect.X;
                int height = rect.Height;
                int width = rect.Width;
                top = this.Bounds.Y;
                left = this.Bounds.Width - width;
                funDemo.StartPosition = FormStartPosition.Manual;
                funDemo.SetBounds(left, top, width, height);

                funDemo.Show();
            }

            if (funDemo != null)
            {
                try
                {
                    if (String.IsNullOrWhiteSpace(address))
                    {
                    }
                    funDemo.FireEventFunDemoLoad("Place", editingWhat, "", "", "", "", "", item, address, city, county, state, zip, phone, location);
                    funDemo.TopMost = true;
                    if (!funDemo.Visible && !funDemo.IsDisposed)
                    {
                        funDemo.Visible = true;
                        funDemo.Refresh();
                    }
                    dr = gridMain6.GetFocusedDataRow();
                    if (dr != null)
                    {
                        dr["data"] = item;
                        gridMain6.RefreshData();
                        gridMain6.RefreshEditor(true);
                    }

                    gridMain6.Focus();
                    popupForm.ListBox.Focus();
                }
                catch (Exception ex)
                {
                    if (funDemo.IsDisposed)
                    {
                        funDemo = new FuneralDemo("Place", "", "", "", "", "", "", "", "", "", "", "");
                        funDemo.FunDemoDone += FunDemo_FunDemoDone6;
                        funDemo.Show();
                        funDemo.Hide();
                    }
                }
            }
            popupForm6.ListBox.Show();
            //cmb.ShowPopup();
        }
        /****************************************************************************************/
        private void ListBox6_MouseMove(object sender, MouseEventArgs e)
        {
            PopupListBox listBoxControl = sender as PopupListBox;
            ComboBoxEdit cmb = listBoxControl.OwnerEdit as ComboBoxEdit;
            int index = listBoxControl.IndexFromPoint(new Point(e.X, e.Y));
            if (index < 0)
            {
                if (e.Y > listBoxControl.Height)
                {
                }
            }
            else
            {
                if (index == lastIndex)
                    return;

                whichRowChanged = gridMain6.FocusedRowHandle;
                DataRow dr = gridMain6.GetFocusedDataRow();

                string columnName = gridMain6.FocusedColumn.FieldName.ToUpper();


                string item = cmb.Properties.Items[index].ToString();
                textBox1.Text = item;
                textBox1.Refresh();
                lastIndex = index;
                //dr["data"] = item; //Ramma Zamma
                //gridMain6.RefreshData();
                //gridMain6.RefreshEditor(true);
                //popupForm.ListBox.Refresh();
                popupForm6.ListBox.Focus();

                if (1 == 1)
                {
                    cmb.ShowPopup();
                    if (funDemo != null && !funDemo.IsDisposed)
                    {
                        if (!funDemo.Visible)
                        {
                            funDemo.Visible = true;
                            funDemo.Show();
                        }
                    }
                    return;
                }

                string answer = item;
                string address = "";
                string city = "";
                string county = "";
                string state = "";
                string zip = "";
                string phone = "";
                string location = "";

                try
                {
                    DataRow[] dRows = null;
                    dRows = itemDt.Select("answer='" + item + "'");
                    if (dRows.Length > 0)
                    {
                        location = dRows[0]["location"].ObjToString();
                        address = dRows[0]["address"].ObjToString();
                        city = dRows[0]["city"].ObjToString();
                        county = dRows[0]["county"].ObjToString();
                        state = dRows[0]["state"].ObjToString();
                        zip = dRows[0]["zip"].ObjToString();
                        phone = dRows[0]["phone"].ObjToString();
                    }
                }
                catch (Exception ex)
                {
                }
                if (funDemo == null)
                {
                    funDemo = new FuneralDemo("Place", "", "", "", "", "", "", "", "", "", "", "");
                    funDemo.FunDemoDone += FunDemo_FunDemoDone6;
                    Rectangle rect = funDemo.Bounds;
                    int top = rect.Y;
                    int left = rect.X;
                    int height = rect.Height;
                    int width = rect.Width;
                    top = this.Bounds.Y;
                    left = this.Bounds.Width - width;
                    funDemo.StartPosition = FormStartPosition.Manual;
                    funDemo.SetBounds(left, top, width, height);

                    funDemo.Show();
                }

                if (funDemo != null)
                {
                    try
                    {
                        funDemo.FireEventFunDemoLoad("Place", editingWhat, "", "", "", "", "", item, address, city, county, state, zip, phone, location);
                        funDemo.TopMost = true;
                        if (!funDemo.Visible && !funDemo.IsDisposed)
                        {
                            funDemo.Visible = true;
                            funDemo.Refresh();
                        }
                        dr = gridMain6.GetFocusedDataRow();
                        if (dr != null)
                        {
                            dr["data"] = item;
                            //gridMain6.RefreshData();
                            //gridMain6.RefreshEditor(true);
                        }

                        //gridMain6.Focus();
                        popupForm6.ListBox.Focus();
                        //this.Focus();
                    }
                    catch (Exception ex)
                    {
                        if (funDemo.IsDisposed)
                        {
                            funDemo = new FuneralDemo("Place", "", "", "", "", "", "", "", "", "", "", "");
                            funDemo.FunDemoDone += FunDemo_FunDemoDone6;
                            funDemo.Show();
                            funDemo.Hide();
                        }
                    }
                }
                cmb.ShowPopup();
            }
        }
        /****************************************************************************************/
        private void ListBox6_MouseDown(object sender, MouseEventArgs e)
        {
            DevExpress.XtraEditors.Popup.ComboBoxPopupListBox box = (DevExpress.XtraEditors.Popup.ComboBoxPopupListBox)sender;
            int selectedRowIndex = box.SelectedIndex;
            if (funDemo != null)
            {
                if (!funDemo.IsDisposed)
                {
                    if (funDemo.Visible)
                    {
                        funDemo.Hide();
                        popupForm6.Close();
                        string item = textBox1.Text.Trim();
                        DataRow dr = gridMain6.GetFocusedDataRow();
                        dr = gridMain6.GetDataRow(whichRowChanged);
                        //dr["data"] = item;
                        //dr["mod"] = "Y";
                        gridMain6.RefreshData();
                        gridMain6.RefreshEditor(true);
                        this.Focus();
                        this.TopMost = true;

                        DataTable tempDt = dr.Table.Copy();
                        string dbField = dr["dbField"].ObjToString();

                        DataTable dx = (DataTable)dgv6.DataSource;
                        DataRow[] dRows = null;
                        dRows = dx.Select("dbfield='" + dbField + "'");
                        if (dRows.Length > 0)
                        {
                            dRows[0]["data"] = item; // ramma zamma
                            dRows[0]["mod"] = "Y";
                            gridMain6.RefreshData();
                            gridMain6.RefreshEditor(true);
                        }
                        if (itemDt != null)
                        {
                            dRows = itemDt.Select("answer='" + item + "'");
                            if (dRows.Length > 0)
                            {
                                string location = dRows[0]["location"].ObjToString();
                                string address = dRows[0]["address"].ObjToString();
                                string city = dRows[0]["city"].ObjToString();
                                string county = dRows[0]["county"].ObjToString();
                                string state = dRows[0]["state"].ObjToString();
                                string zip = dRows[0]["zip"].ObjToString();
                                string phone = dRows[0]["phone"].ObjToString();

                                string reference = "";

                                dRows = dx.Select("reference LIKE '" + dbField + "~%'");
                                if (dRows.Length > 0)
                                {
                                    for (int i = 0; i < dRows.Length; i++)
                                    {
                                        reference = dRows[i]["reference"].ObjToString();
                                        string answer = ProcessReference(reference, address, city, county, state, zip, phone);
                                        dRows[i]["data"] = answer;
                                        dRows[i]["mod"] = "Y";
                                    }
                                }
                            }
                        }
                        funModified = true;
                        otherModified = true;
                        btnSaveAll.Show();
                        btnSaveAll.Refresh();
                    }
                }
            }
        }
        /****************************************************************************************/
        private void FunDemo_FunDemoDone6(string title, string firstName, string middleName, string lastName, string suffix, string name, string address, string city, string county, string state, string zip, string phone)
        {
            DataRow dr = gridMain6.GetFocusedDataRow();
            if (dr == null)
                return;
            if (whichRowChanged < 0)
                return;

            int rowHandle = whichRowChanged;
            //if (rowHandle >= 0)
            //    dr = gridMain6.GetDataRow(rowHandle);

            int row = gridMain6.GetFocusedDataSourceRowIndex();

            DataTable dt = (DataTable)dgv6.DataSource;

            if (!String.IsNullOrWhiteSpace(name))
                dr["data"] = name;
            //dr["depPrefix"] = title;
            //dr["depFirstName"] = firstName;
            //dr["depMI"] = middleName;
            //dr["depLastName"] = lastName;
            //dr["depSuffix"] = suffix;
            //dr["address"] = address;
            //dr["city"] = city;
            //dr["county"] = county;
            //dr["state"] = state;
            //dr["zip"] = zip;
            //dr["phone"] = phone;
            //dr["mod"] = "Y";

            funModified = true;
            btnSaveAll.Show();
            btnSaveAll.Refresh();
            dgv6.Refresh();

            funDemo.Hide();
        }
        /***************************************************************************************/
        private void CiLookup6_SelectedIndexChanged(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv6.DataSource;
            DataRow dr = gridMain6.GetFocusedDataRow();
            int rowhandle = gridMain6.FocusedRowHandle;
            int row = gridMain6.GetDataSourceRowIndex(rowhandle);

            try
            {
                string help = dt.Rows[row]["help"].ObjToString();
                string dbField = dt.Rows[row]["dbField"].ObjToString();
                string myData = dt.Rows[row]["data"].ObjToString().Trim();
                myData = G1.protect_data(myData);
                string str = "";

                ComboBoxEdit combo = (ComboBoxEdit)sender;
                //            string what = combo.Text.Trim().ToUpper();
                string what = combo.Text.Trim();
                what = G1.protect_data(what);
                //dr["data"] = what; // ramma zamma

                funModified = true;
                btnSaveAll.Show();

                DataTable tempDt = null;

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

                            string locations = findLocationAssociation();


                            dR = trackDt.Select("tracking='" + dbField.Trim() + "' AND answer='" + what.Trim() + "' AND ( (" + locations + ") " + " OR location='All' )");
                            if (dR.Length > 0)
                            {
                                tempDt = dR.CopyToDataTable();
                            }
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
            catch ( Exception ex )
            {
            }
        }
        /***************************************************************************************/
        private void CiLookup6_SelectedIndexChangedAgain ( string what )
        {
            DataTable dt = (DataTable)dgv6.DataSource;
            DataRow dr = gridMain6.GetFocusedDataRow();
            int rowhandle = gridMain6.FocusedRowHandle;
            int row = gridMain6.GetDataSourceRowIndex(rowhandle);

            try
            {
                string help = dt.Rows[row]["help"].ObjToString();
                string dbField = dt.Rows[row]["dbField"].ObjToString();
                string myData = dt.Rows[row]["data"].ObjToString().Trim();
                myData = G1.protect_data(myData);
                string str = "";

                //ComboBoxEdit combo = (ComboBoxEdit)sender;
                //            string what = combo.Text.Trim().ToUpper();
                //string what = combo.Text.Trim();
                what = G1.protect_data(what);
                //dr["data"] = what; // ramma zamma

                funModified = true;
                btnSaveAll.Show();

                DataTable tempDt = null;

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

                            string locations = findLocationAssociation();


                            dR = trackDt.Select("tracking='" + dbField.Trim() + "' AND answer='" + what.Trim() + "' AND ( (" + locations + ") " + " OR location='All' )");
                            if (dR.Length > 0)
                            {
                                tempDt = dR.CopyToDataTable();
                            }
                            answer = ProcessReference(dR, field);
                            dRows[i]["data"] = answer.Trim();
                            dRows[i]["mod"] = "Y";
                        }
                    }
                    dt.AcceptChanges();
                }
            }
            catch (Exception ex)
            {
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
            answer = answer.Trim();
            answer = answer.TrimStart(',');
            return answer;
        }
        /***************************************************************************************/
        private string ProcessReference(string field, string address, string city, string county, string state, string zip, string phone )
        {
            string answer = "";
            if (String.IsNullOrWhiteSpace(field))
                return answer;
            try
            {
                string[] Lines = null;
                string str = "";
                if (field.IndexOf("~") >= 0)
                {
                    Lines = field.Split('~');
                    if (Lines.Length <= 1)
                        return answer;
                    field = Lines[1];
                }

                if (field.IndexOf("+") < 0)
                {
                    //answer = dR[index][field].ObjToString();
                    //answer = field;
                    if (!String.IsNullOrWhiteSpace(field))
                    {
                        if (field.ToUpper().IndexOf("ADDRESS") >= 0)
                            answer = address;
                        else if (field.ToUpper().IndexOf("CITY") >= 0)
                            answer = city;
                        else if (field.ToUpper().IndexOf("COUNTY") >= 0)
                            answer = county;
                        else if (field.ToUpper().IndexOf("STATE") >= 0)
                            answer = state;
                        else if (field.ToUpper().IndexOf("ZIP") >= 0)
                            answer = zip;
                        else if (field.ToUpper().IndexOf("PHONE") >= 0)
                            answer = phone;
                    }
                }
                else
                {
                    Lines = field.Split('+');
                    str = "";
                    for (int j = 0; j < Lines.Length; j++)
                    {
                        field = Lines[j].Trim();
                        try
                        {
                            if (!String.IsNullOrWhiteSpace(field))
                            {
                                if (field.ToUpper().IndexOf("ADDRESS") >= 0)
                                {
                                    str = address;
                                    answer += str + " ";
                                }
                                else if (field.ToUpper().IndexOf("CITY") >= 0)
                                {
                                    str = city;
                                    answer += str + " ";
                                }
                                else if (field.ToUpper().IndexOf("COUNTY") >= 0)
                                {
                                    str = county;
                                    answer += str + " ";
                                }
                                else if (field.ToUpper().IndexOf("STATE") >= 0)
                                {
                                    str = state;
                                    answer += str + " ";
                                }
                                else if (field.ToUpper().IndexOf("ZIP") >= 0)
                                {
                                    str = zip;
                                    answer += str + " ";
                                }
                                else if (field.ToUpper().IndexOf("PHONE") >= 0)
                                {
                                    str = phone;
                                    answer += str + " ";
                                }
                                else if (field.ToUpper().IndexOf(",") >= 0)
                                {
                                    answer = answer.Trim();
                                    if ( !String.IsNullOrWhiteSpace ( answer ))
                                        answer += field + " ";
                                }
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

            what = G1.protect_data(what);

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
            {
                dr["add"] = "";
                dr["edit"] = "";
                return;
            }

            what = G1.protect_data(what);

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
                    if ( ciLookup.Items.Count <= 0 && myDt.Rows.Count > 0 )
                    {
                        dRows = myDt.Select("stuff='" + what + "'");
                        if (dRows.Length > 0)
                        {
                            for (int i = 0; i < dRows.Length; i++)
                            {
                                answers = dRows[i]["stuff"].ObjToString().Trim();
                                if ( !String.IsNullOrWhiteSpace ( answers))
                                    ciLookup.Items.Add ( answers );
                            }
                        }
                    }
                    for (int i = 0; i < ciLookup.Items.Count; i++)
                    {
                        answers = ciLookup.Items[i].ObjToString();
                        if (what.Trim().ToUpper() == answers.Trim().ToUpper())
                        {
                            found = true;
                        }
                    }
                    if ( !found && myDt.Rows.Count > 0 )
                    {
                        dRows = myDt.Select("stuff='" + what + "'");
                        if (dRows.Length > 0)
                        {
                            for (int i = 0; i < dRows.Length; i++)
                            {
                                answers = dRows[i]["stuff"].ObjToString().Trim();
                                if ( !String.IsNullOrWhiteSpace ( answers ))
                                    ciLookup.Items.Add ( answers );
                            }
                        }
                        for (int i = 0; i < ciLookup.Items.Count; i++)
                        {
                            answers = ciLookup.Items[i].ObjToString();
                            if (what.Trim().ToUpper() == answers.Trim().ToUpper())
                            {
                                found = true;
                            }
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
                                    what = G1.protect_data(what);
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

            newField = field.ToUpper().Replace("LOCATION", "COUNTY");
            dRows = dt6.Select("field='" + newField + "'");
            if (dRows.Length > 0)
            {
                reference = dRows[0]["reference"].ObjToString();
                dbField = dRows[0]["dbField"].ObjToString();
                answer = ProcessReference(dR, reference);
                dRows[0]["data"] = answer;
                //                dRows[0]["data"] = dR[0]["zip"].ObjToString();
            }

            newField = field.ToUpper().Replace("LOCATION", "PHONE");
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
        public static void ConfirmCustExtended(string workContract, string serviceId = "", string serviceDate = "", string custExtendedFile = "", string county = "", string insideCity = "", string arrangementDate = "")
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


            G1.update_db_table(custExtendedFile, "record", record, new string[] { "contractNumber", workContract, "serviceId", serviceId, "serviceDate", serviceDate, "serviceLoc", loc, "srvDate", srvDate, "DECCOUNTY", county, "IN CITY LIMITS", insideCity, "caseCreatedDate", caseCreatedDate, "arrangementDate", arrangementDate });
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
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                mod = dt.Rows[i]["mod"].ObjToString();
                if (mod == "Y")
                {
                    dbfield = dt.Rows[i]["dbfield"].ObjToString();
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
                    G1.update_db_table(custExtendedFile, "record", record, myList);
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** Updating Extended Data for Contract " + workContract + " Error " + ex.Message.ToString());
            }
        }
        /****************************************************************************************/
        private DataTable LoadEmptyRelatives()
        {
            string cmd = "Select * from `relatives` where `contractNumber` = 'XYZZYABC'";
            DataTable dt = G1.get_db_data(cmd);
            return dt;
        }
        /***************************************************************************************/
        public bool trackChange = true;
        public string whichTab = "MAIN";
        public string mainTab = "";
        public int mainRow = 0;
        public string otherTab = "";
        public int otherRow = 0;
        public void FireEventFunShowMain()
        {
            trackChange = false;
            whichTab = "MAIN";

            AddRemoveTabPage(tabFamily, true);
            AddRemoveTabPage(tabLegal, true);

            AddRemoveTabPage(tabFuneralData, false);
            AddRemoveTabPage(tabPallbearers, false);
            AddRemoveTabPage(tabHonoraryPallBearers, false);
            AddRemoveTabPage(tabClergy, false);
            AddRemoveTabPage(tabMusicians, false);
            AddRemoveTabPage(tabDisclosures, false);

            FireEventFunShowTab("MAIN");

            trackChange = true;

            this.Refresh();
        }
        /***************************************************************************************/
        public void FireEventFunShowOthers()
        {
            trackChange = false;
            whichTab = "OTHER";

            AddRemoveTabPage(tabFamily, false);
            AddRemoveTabPage(tabLegal, false);

            AddRemoveTabPage(tabFuneralData, true);
            AddRemoveTabPage(tabPallbearers, true);
            AddRemoveTabPage(tabHonoraryPallBearers, true);
            AddRemoveTabPage(tabClergy, true);
            AddRemoveTabPage(tabMusicians, true);
            AddRemoveTabPage(tabDisclosures, true);

            FireEventFunShowTab("OTHER");
            if (dgv6.Visible)
            {
                gridMain6.ExpandAllGroups();
                gridMain6.Focus();
                dgv6.Focus(); // This is very important because it fixes a problem that caused the funeral screen to position back to the top should the user scroll and then click the mouse.
            }

            trackChange = true;

            this.Refresh();
        }
        /***************************************************************************************/
        public void AddRemoveTabPage(TabPage page, bool add)
        {
            if (add)
            {
                if (!tabControl1.Contains(page))
                    tabControl1.TabPages.Add(page);
            }
            else
            {
                if (tabControl1.Contains(page))
                    tabControl1.TabPages.Remove(page);
            }
        }
        /****************************************************************************************/
        public void FireEventFunShowTab(string whichOne)
        {
            int rowHandle = 0;
            if (whichOne == "MAIN")
            {
                if (String.IsNullOrWhiteSpace(mainTab))
                {
                    tabControl1.SelectedIndex = 0;

                    gridMainDep.ClearSelection();
                    gridMainDep.SelectRow(g1);
                    gridMainDep.FocusedRowHandle = g1;
                    gridMainDep.RefreshData();
                    dgvDependent.Refresh();
                }
                else if (mainTab == "tabLegal")
                {
                    tabControl1.SelectedIndex = 1;

                    gridMainLegal.ClearSelection();
                    gridMainLegal.SelectRow(g2);
                    gridMainLegal.FocusedRowHandle = g2;
                    gridMainLegal.RefreshData();
                    dgvLegal.Refresh();
                }
            }
            else
            {
                if (String.IsNullOrWhiteSpace(otherTab))
                    tabControl1.SelectedIndex = 0;
                else if (otherTab == "tabFuneralData")
                {
                    tabControl1.SelectedIndex = 0;

                    gridMain6.ClearSelection();
                    gridMain6.SelectRow(g3);
                    gridMain6.FocusedRowHandle = g3;
                    if (dgv6.Visible)
                    {
                        gridMain6.RefreshData();
                        dgv6.Refresh();
                        gridMain6.ExpandAllGroups();
                    }
                }
                else if (otherTab == "tabMusicians")
                {
                    tabControl1.SelectedIndex = 4;
                }
                else if (otherTab == "tabClergy")
                {
                    tabControl1.SelectedIndex = 3;
                }
                else if (otherTab == "tabDisclosures")
                {
                    tabControl1.SelectedIndex = 5;
                }
                else if (otherTab == "tabPallbearers")
                {
                    tabControl1.SelectedIndex = 1;

                    gridMain2.ClearSelection();
                    gridMain2.SelectRow(g4);
                    gridMain2.FocusedRowHandle = g4;
                    gridMain2.RefreshData();
                    dgv2.Refresh();
                }
                else if (otherTab == "tabHonoraryPallBearers")
                {
                    tabControl1.SelectedIndex = 2;

                    gridMain3.ClearSelection();
                    gridMain3.SelectRow(g5);
                    gridMain3.FocusedRowHandle = g5;
                    gridMain3.RefreshData();
                    dgv3.Refresh();
                }
            }
        }
        /****************************************************************************************/
        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            int rowHandle = 0; // Ramma Zamma
            TabPage current = (sender as TabControl).SelectedTab;
            if (current == null)
                return;

            if (trackChange)
            {
                if (whichTab == "MAIN")
                {
                    mainTab = current.Name;
                    if (mainTab == "tabFamily")
                    {
                        if (dgvDependent.Visible)
                        {

                            gridMainDep.ClearSelection();
                            gridMainDep.FocusedRowHandle = g1;
                            gridMainDep.SelectRow(g1);
                            gridMainDep.RefreshData();
                        }
                    }
                    else
                    {
                        if (dgvLegal.Visible)
                        {
                            gridMainLegal.ClearSelection();
                            gridMainLegal.FocusedRowHandle = g2;
                            gridMainLegal.SelectRow(g2);
                            gridMainLegal.RefreshData();
                        }
                    }
                }
                else
                {
                    otherTab = current.Name;
                    if (otherTab.ToUpper() == "TABFUNERALDATA")
                    {
                        if (dgv6.Visible)
                        {
                            gridMain6.ClearSelection();
                            gridMain6.FocusedRowHandle = g3;
                            gridMain6.SelectRow(g3);
                            gridMain6.RefreshData();
                            gridMain6.ExpandAllGroups();
                        }
                    }
                    else if (otherTab.ToUpper() == "TABPALLBEARERS")
                    {
                        if (dgv2.Visible)
                        {
                            gridMain2.ClearSelection();
                            gridMain2.FocusedRowHandle = g4;
                            gridMain2.SelectRow(g4);
                            gridMain2.RefreshData();
                            gridMain2.ExpandAllGroups();
                        }
                    }
                    else if (otherTab.ToUpper() == "TABHONORARYPALLBEARERS")
                    {
                        if (dgv3.Visible)
                        {
                            gridMain3.ClearSelection();
                            gridMain3.FocusedRowHandle = g5;
                            gridMain3.SelectRow(g5);
                            gridMain3.RefreshData();
                            gridMain3.ExpandAllGroups();
                        }
                    }
                }
            }
            //if (workLegal)
            //    return;
            //addSignatureToolStripMenuItem.Enabled = false;
            //contextMenuStrip1.Enabled = false;
            if (current.Name.Trim().ToUpper() == "TABFAMILY")
            {
                ciLookup.Items.Clear();
                chkLegal.Hide();
                lblSelect.Hide();
                pictureBox11.Show();
                pictureBox12.Show();
                picRowUp.Show();
                picRowDown.Show();
                //addSignatureToolStripMenuItem.Enabled = false;
                lblFamily.Text = "All Family Members :";
                DataTable dt = (DataTable)dgvDependent.DataSource;
                if (dt == null)
                    dt = LoadEmptyRelatives();
                G1.NumberDataTable(dt);

                gridMainDep.ClearSelection();
                gridMainDep.FocusedRowHandle = g1;
                gridMainDep.SelectRow(g1);
                gridMainDep.RefreshData();
            }
            else if (current.Name.Trim().ToUpper() == "TABLEGAL")
            {
                ciLookup.Items.Clear();
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

                //CleanupLegal(legDt);

                G1.NumberDataTable(legDt);

                dgvLegal.DataSource = legDt;
                addSignatureToolStripMenuItem.Enabled = true;
                contextMenuStrip1.Enabled = true;
                lblFamily.Text = "Select Legal Members :";

                if (dgvLegal.Visible && whichTab == "MAIN")
                {
                    gridMainLegal.ClearSelection();
                    gridMainLegal.FocusedRowHandle = g2;
                    gridMainLegal.SelectRow(g2);
                    gridMainLegal.RefreshData();
                }
            }
            else if (current.Name.Trim().ToUpper() == "TABFUNERALDATA")
            {
                ciLookup.Items.Clear();
                chkLegal.Hide();
                lblSelect.Hide();
                pictureBox11.Hide();
                pictureBox12.Hide();
                picRowUp.Hide();
                picRowDown.Hide();
                //addSignatureToolStripMenuItem.Enabled = false;
                lblFamily.Text = "Enter Other Funeral Data :";

                if (dgv6.Visible && whichTab != "MAIN")
                {
                    gridMain6.ClearSelection();
                    gridMain6.FocusedRowHandle = g3;
                    gridMain6.SelectRow(g3);
                    gridMain6.RefreshData();
                    gridMain6.ExpandAllGroups();
                }
            }
            else if (current.Name.Trim().ToUpper() == "TABPALLBEARERS")
            {
                ciLookup.Items.Clear();
                chkLegal.Hide();
                lblSelect.Hide();
                pictureBox11.Show();
                pictureBox12.Show();
                picRowUp.Show();
                picRowDown.Show();
                addSignatureToolStripMenuItem.Enabled = false;
                lblFamily.Text = "Enter Pall Bearers :";

                gridMain2.ClearSelection();
                gridMain2.FocusedRowHandle = g4;
                gridMain2.SelectRow(g4);
                gridMain2.RefreshData();
                gridMain2.ExpandAllGroups();
            }
            else if (current.Name.Trim().ToUpper() == "TABHONORARYPALLBEARERS")
            {
                ciLookup.Items.Clear();
                chkLegal.Hide();
                lblSelect.Hide();
                pictureBox11.Show();
                pictureBox12.Show();
                picRowUp.Show();
                picRowDown.Show();
                addSignatureToolStripMenuItem.Enabled = false;
                lblFamily.Text = "Honorary Pall Bearers :";
                DataTable dt = (DataTable)dgv3.DataSource;
                //if (dt.Rows.Count <= 0)
                //    LoadHonoraryPallBearers(editCust.masterTable);

                gridMain3.ClearSelection();
                gridMain3.FocusedRowHandle = g5;
                gridMain3.SelectRow(g5);
                gridMain3.RefreshData();
                gridMain3.ExpandAllGroups();
            }
            else if (current.Name.Trim().ToUpper() == "TABCLERGY")
            {
                ciLookup.Items.Clear();
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
                ciLookup.Items.Clear();
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
                ciLookup.Items.Clear();
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
        private void CleanupLegal(DataTable dt)
        {
            string relation = "";
            for (int i = (dt.Rows.Count - 1); i >= 0; i--)
            {
                relation = dt.Rows[i]["depRelationship"].ObjToString().ToUpper();
                if (relation == "DISCLOSURES")
                    dt.Rows.RemoveAt(i);
            }
        }
        /***********************************************************************************************/
        private void MergePallBearers()
        {
            if (newFamilyPB)
                return;
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
            if (newFamilyPB)
                return;
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
                    dt.Rows[i]["authEmbalming"] = "0";

                text = dt.Rows[i]["authCremation"].ObjToString();
                if (!String.IsNullOrWhiteSpace(text))
                    dt.Rows[i]["authCremation"] = text;
                else
                    dt.Rows[i]["authCremation"] = "0";

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
        private DataTable itemDt = null;
        //private string editingWhat = "";
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
            string ddata = dr["data"].ObjToString();
            string currentData = "";
            string field = "";
            isProtected = false;

            ciLookup6.Items.Clear();
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
                {
                    string locations = findLocationAssociation();

                    //dR = trackDt.Select("tracking='" + dbField + "' AND ( location='" + EditCust.activeFuneralHomeName + "' ) ");
                    dR = trackDt.Select("tracking='" + dbField + "' AND ( " + locations + " ) ");
                }
                else
                    dR = trackDt.Select("tracking='" + dbField + "'");
                if (dR.Length > 0)
                    itemDt = dR.CopyToDataTable();

                ciLookup6.Items.Clear();
                if (dR.Length > 0)
                {
                    for (int i = 0; i < dR.Length; i++)
                        AddToMyDt(dR[i]["answer"].ObjToString());
                }
                dR = trackDt.Select("tracking='" + dbField + "' AND ( location='All' ) ");
                if (dR.Length > 0)
                {
                    DataTable mergeDt = dR.CopyToDataTable();
                    if (itemDt == null)
                        itemDt = mergeDt.Copy();
                    else
                        itemDt.Merge(mergeDt);
                    for (int i = 0; i < dR.Length; i++)
                        AddToMyDt(dR[i]["answer"].ObjToString());
                }
                if (clergyDt != null)
                {
                    myDt.Rows.Clear();
                    string prefix = "";
                    string suffix = "";
                    string fName = "";
                    string lName = "";
                    string mi = "";
                    string name = "";
                    string clergy = "";
                    bool found = false;
                    for (int i = 0; i < clergyDt.Rows.Count; i++)
                    {
                        prefix = clergyDt.Rows[i]["depPrefix"].ObjToString();
                        suffix = clergyDt.Rows[i]["depSuffix"].ObjToString();
                        fName = clergyDt.Rows[i]["depFirstName"].ObjToString();
                        lName = clergyDt.Rows[i]["depLastName"].ObjToString();
                        mi = clergyDt.Rows[i]["depMi"].ObjToString();

                        clergy = BuildFullName(prefix, fName, mi, lName, suffix);
                        //clergy = lName;
                        //clergy = fName + " " + lName;
                        //fName = clergyDt.Rows[i]["depPrefix"].ObjToString();
                        //lName = clergyDt.Rows[i]["fullName"].ObjToString();
                        //if (!String.IsNullOrWhiteSpace(fName))
                        //    clergy = fName + " " + lName;
                        //else
                        //    clergy = lName;
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
                            if (dbField.ToUpper() == "SRVDATE" || dbField.ToUpper() == "SRV2DATE")
                                CheckForSpecialDay(myDate.ObjToDateTime());

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
                            if (dbField.ToUpper() == "SRVDATE")
                            {
                            }
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
                    if (funDemo != null)
                    {
                        if (funDemo.IsDisposed)
                        {
                            currentData = dr["data"].ObjToString();
                            BringFunDemoUp(currentData);
                        }
                        else if (!funDemo.Visible)
                        {
                            currentData = dr["data"].ObjToString();
                            //funDemo.FireEventFunDemoLoad("Place", editingWhat, "", "", "", "", "", currentData.Trim(), "", "", "", "", "", "", "");
                            //funDemo.Visible = true;
                            //funDemo.Show();
                            //funDemo.Refresh();
                        }
                    }
                }
                DataView tempview = myDt.DefaultView;
                tempview.Sort = "stuff asc";
                myDt = tempview.ToTable();

                if (myDt.Rows.Count > 0)
                {
                    //var newDt = myDt.AsEnumerable()
                    // .GroupBy(x => x.Field<string>("stuff"))
                    // .Select(y => y.First())
                    // .CopyToDataTable();
                    //myDt = newDt.Copy();

                    myDt = RemoveDuplicates(myDt, "stuff");
                }

                string stuff = "";
                for (int i = 0; i < myDt.Rows.Count; i++)
                {
                    stuff = myDt.Rows[i]["stuff"].ObjToString().Trim();
                    if ( !String.IsNullOrWhiteSpace ( stuff ))
                        ciLookup6.Items.Add ( stuff );
                }

                gridMain6.Columns["data"].ColumnEdit = ciLookup6;
                gridMain6.RefreshData();
                gridMain6.RefreshEditor(true);

                //if ( String.IsNullOrWhiteSpace ( currentData) && !String.IsNullOrWhiteSpace ( holdKey ))
                //{
                //    dr["data"] = holdKey;
                //    holdKey = "";
                //    gridMain6.RefreshData();
                //    gridMain6.RefreshEditor(true);
                //}
            }
        }
        /****************************************************************************************/
        private DataTable holidayDt = null;
        private void CheckForSpecialDay(DateTime date)
        {
            bool special = false;
            if (date.DayOfWeek == DayOfWeek.Sunday)
                special = true;
            else if (date.DayOfWeek == DayOfWeek.Saturday)
                special = true;

            if (special)
            {
                MessageBox.Show("*** INFO *** This day is on a weekend.\nMake certain you added weekend charges!", "Weekend Dialog", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                return;
            }
            if (holidayDt == null)
                holidayDt = G1.get_db_data("Select * from `holidays`;");

            string sDate = date.ToString("MM/dd/yyyy");
            string dDate = "";
            string holiday = "";

            try
            {
                for (int i = 0; i < holidayDt.Rows.Count; i++)
                {
                    dDate = holidayDt.Rows[i]["date"].ObjToDateTime().ToString("MM/dd/yyyy");
                    if (dDate == sDate)
                    {
                        holiday = holidayDt.Rows[i]["holiday"].ObjToString();
                        MessageBox.Show("*** INFO *** This day appears to be a holiday (" + holiday + ").\nMake certain you added Holiday charges!", "Holiday Dialog", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }
        /****************************************************************************************/
        private void AddToMyDt(string data)
        {
            if (String.IsNullOrWhiteSpace(data))
                return;
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
        public static string BuildFullName(string prefix, string firstName, string mi, string lastName, string suffix)
        {
            string fullName = prefix;
            if (!String.IsNullOrWhiteSpace(fullName))
                fullName += " ";
            fullName += firstName;
            if (!String.IsNullOrWhiteSpace(firstName))
                fullName += " ";
            fullName += mi;
            if (!String.IsNullOrWhiteSpace(mi))
                fullName += " ";
            fullName += lastName;
            if (!String.IsNullOrWhiteSpace(lastName) && !String.IsNullOrWhiteSpace(suffix))
                fullName += ", ";
            fullName += suffix;
            return fullName;
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
                if (data.Trim() == "+") // Add New Tracking
                {
                    try
                    {
                        string field = dt.Rows[rowHandle]["dbfield"].ObjToString();
                        data = dt.Rows[rowHandle]["data"].ObjToString();
                        if (String.IsNullOrWhiteSpace(data))
                            return;

                        data = G1.protect_data(data);
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
                    catch ( Exception ex)
                    {
                        MessageBox.Show("***ERROR*** Data Error.\n" + ex.Message.ToString(), "Data Entry Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                    }
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
        private string editingWhat = "";
        private void gridMain4_ShownEditor(object sender, EventArgs e)
        {
            isProtected = false;
            GridColumn currCol = gridMain4.FocusedColumn;
            currentColumn = currCol.FieldName;
            if (currentColumn.ToUpper() != "DEPLASTNAME" && currentColumn.ToUpper() != "DEPFIRSTNAME" )
                return;

            int focusedRow = gridMain4.FocusedRowHandle;
            int row = gridMain4.GetDataSourceRowIndex(focusedRow);

            DataRow dr = gridMain4.GetFocusedDataRow();
            string field = dr[currentColumn].ObjToString();
            if (field == "Decedent’s Church Affiliation")
            {
            }
            editingWhat = field;

            //string dbField = dr["dbfield"].ObjToString();
            //string help = dr["reference"].ObjToString();

            isProtected = false;

            try
            {
                //ciLookup.Items.Clear();
                //if (myDt == null)
                //{
                //    myDt = new DataTable();
                //    myDt.Columns.Add("stuff");
                //}
                //myDt.Rows.Clear();

                //string[] Lines = null;
                //string cmd = "";
                //if (help.Length > 0)
                //    cmd = help.Substring(0, 1);
                //if (cmd == "$")
                //{
                //    Lines = help.Split('=');
                //    if (Lines.Length < 2)
                //        return;
                //}

                //if (help.Trim().ToUpper() == "/PROTECT")
                //    isProtected = true;
                //DataRow[] dR = trackingDt.Select("tracking='" + dbField + "'");
                //if (dR.Length > 0)
                //{
                //    string substitute = dR[0]["using"].ObjToString();
                //    if (!String.IsNullOrWhiteSpace(substitute))
                //        dbField = substitute;
                //    if (!String.IsNullOrWhiteSpace(EditCust.activeFuneralHomeName))
                //    {
                //        string locations = FunFamilyNew.findLocationAssociation();
                //        //dR = trackDt.Select("tracking='" + dbField + "' AND ( location='" + EditCust.activeFuneralHomeName + "' ) ");
                //        dR = trackDt.Select("tracking='" + dbField + "' AND (" + locations + ")");
                //    }
                //    else
                //        dR = trackDt.Select("tracking='" + dbField + "'");
                //    if (dR.Length > 0)
                //        itemDt = dR.CopyToDataTable();
                //    for (int i = 0; i < dR.Length; i++)
                //        AddToMyDt(dR[i]["answer"].ObjToString());
                //    dR = trackDt.Select("tracking='" + dbField + "' AND ( location='All' ) ");
                //    if (dR.Length > 0)
                //    {
                //        DataTable mergeDt = dR.CopyToDataTable();
                //        itemDt.Merge(mergeDt);
                //        for (int i = 0; i < dR.Length; i++)
                //            AddToMyDt(dR[i]["answer"].ObjToString());
                //    }
                //}
            }
            catch (Exception ex)
            {
            }
            if (myDt == null)
            {
                gridMain4.RefreshData();
                gridMain4.RefreshEditor(true);
                return;
            }

            if (myDt.Rows.Count <= 0)
            {
                //gridMain6.Columns["data"].ColumnEdit = null;
                //string type = dr["type"].ObjToString();
                gridMain4.RefreshData();
                gridMain4.RefreshEditor(true);
            }
            else
            {
                //if (lastDb != dbField)
                //{
                //    lastDb = dbField;
                //    if (gridMain6.Columns["data"].ColumnEdit != null)
                //    {
                //        gridMain6.Columns["data"].ColumnEdit = null;
                //    }
                //    //textBox1.Text = "Junk";
                //    if (funDemo != null)
                //    {
                //        if (funDemo.IsDisposed)
                //        {
                //            string currentData = dr["data"].ObjToString();
                //            BringFunDemoUp(currentData);
                //        }
                //        else if (!funDemo.Visible)
                //        {
                //            string currentData = dr["data"].ObjToString();
                //            funDemo.FireEventFunDemoLoad("Place", editingWhat, "", "", "", "", "", currentData.Trim(), "", "", "", "", "", "", "");
                //            funDemo.Visible = true;
                //            funDemo.Show();
                //            funDemo.Refresh();
                //        }
                //    }
                //}

                //DataView tempview = myDt.DefaultView;
                //tempview.Sort = "stuff asc";
                //myDt = tempview.ToTable();

                //myDt = FunFamilyNew.RemoveDuplicates(myDt, "stuff");

                //for (int i = 0; i < myDt.Rows.Count; i++)
                //    ciLookup.Items.Add(myDt.Rows[i]["stuff"].ObjToString());

                //gridMain6.Columns["data"].ColumnEdit = ciLookup;
                //gridMain6.RefreshData();
                //gridMain6.RefreshEditor(true);
            }
        }
        //private void gridMain4_ShownEditorx(object sender, EventArgs e)
        //{
        //    GridColumn currCol = gridMain4.FocusedColumn;
        //    currentColumn = currCol.FieldName;
        //    if (currentColumn == "depPrefix" || currentColumn == "depFirstName" || currentColumn == "depMI" || currentColumn == "depLastName" || currentColumn == "depSuffix")
        //    { // ramma
        //        DataTable dt = (DataTable)dgv4.DataSource;
        //        int rowHandle = gridMain4.FocusedRowHandle;
        //        int row = gridMain4.GetDataSourceRowIndex(rowHandle);
        //        string fullName = dt.Rows[row]["depPrefix"].ObjToString();
        //        if (!String.IsNullOrWhiteSpace(fullName))
        //            fullName += " " + dt.Rows[row]["depFirstName"].ObjToString() + " " + dt.Rows[row]["depLastName"].ObjToString();
        //        else
        //            fullName = dt.Rows[row]["depFirstName"].ObjToString() + " " + dt.Rows[row]["depLastName"].ObjToString();
        //        //fullName = BuildSelectClergyName(dt, row);
        //        dt.Rows[row]["fullName"] = fullName;
        //        currentColumn = "fullName";
        //    }
        //    if (currentColumn.ToUpper() != "FULLNAME")
        //        return;
        //    DataRow dr = gridMain4.GetFocusedDataRow();
        //    string dbField = "SRVClergy";
        //    DataRow[] dR = trackingDt.Select("tracking='" + dbField + "'");
        //    if (dR.Length <= 0)
        //    {
        //        //                gridMain4.Columns["fullName"].ColumnEdit = null;
        //        gridMain4.Columns["depLastName"].ColumnEdit = null;
        //        return;
        //    }

        //    string locations = findLocationAssociation();

        //    if (locations.IndexOf(" OR ") > 0)
        //    {
        //        try
        //        {
        //            dR = trackDt.Select("tracking='" + dbField + "' AND (" + locations + ")");
        //        }
        //        catch (Exception ex)
        //        {
        //        }
        //    }
        //    else
        //        dR = trackDt.Select("tracking='" + dbField + "' AND location='" + EditCust.activeFuneralHomeName + "'");

        //    DataTable ddR = null;
        //    if (dR.Length > 0)
        //    {
        //        string name = "";
        //        string prefix = "";
        //        string firstName = "";
        //        string miName = "";
        //        string lastName = "";
        //        string suffix = "";

        //        ddR = dR.CopyToDataTable();

        //        try
        //        {
        //            for (int i = 0; i < ddR.Rows.Count; i++)
        //            {
        //                name = ddR.Rows[i]["answer"].ObjToString();
        //                if (String.IsNullOrWhiteSpace(name))
        //                    continue;
        //                firstName = ddR.Rows[i]["depFirstName"].ObjToString();
        //                lastName = ddR.Rows[i]["depLastName"].ObjToString();
        //                if (String.IsNullOrWhiteSpace(lastName))

        //                    G1.ParseOutName(name, ref prefix, ref firstName, ref lastName, ref miName, ref suffix);
        //                name = firstName + " " + lastName;
        //                ddR.Rows[i]["answer"] = name;
        //            }

        //            DataView tempview = ddR.DefaultView;
        //            tempview.Sort = "answer asc";
        //            ddR = tempview.ToTable();

        //            ddR = RemoveDuplicates(ddR, "answer");

        //        }
        //        catch (Exception ex)
        //        {
        //        }
        //    }
        //    ciLookup.Items.Clear();
        //    repositoryItemComboBox21.Items.Clear();
        //    if (ddR == null)
        //        return;
        //    if (ddR.Rows.Count <= 0)
        //        return;
        //    for (int i = 0; i < ddR.Rows.Count; i++)
        //    {
        //        ciLookup.Items.Add(ddR.Rows[i]["answer"].ObjToString());
        //        repositoryItemComboBox21.Items.Add(ddR.Rows[i]["answer"].ObjToString());
        //    }
        //    try
        //    {

        //        //gridMain4.Columns["depLastName"].ColumnEdit = ciLookup;
        //    }
        //    catch (Exception ex)
        //    {
        //    }
        //}
        /****************************************************************************************/
        public static DataTable RemoveDuplicates(DataTable dt, string columnName, string column2 = "")
        {
            DataTable newDt = dt.Copy();
            try
            {
                if (!String.IsNullOrWhiteSpace(column2))
                {
                    newDt = dt.AsEnumerable()
                                     .GroupBy(x => x.Field<string>(columnName) + " " + x.Field<string>(column2))
                                     .Select(y => y.First())
                                     .CopyToDataTable();
                }
                else
                {
                    newDt = dt.AsEnumerable()
                                     .GroupBy(x => x.Field<string>(columnName))
                                     .Select(y => y.First())
                                     .CopyToDataTable();
                }
            }
            catch (Exception ex)
            {
            }
            return newDt;
        }
        /****************************************************************************************/
        public static string findLocationAssociation()
        {
            string location = EditCust.activeFuneralHomeName;
            string cmd = "Select * from `associations`;";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                return "location='" + EditCust.activeFuneralHomeName + "'";

            DataRow[] dRows = dt.Select("locations LIKE '%" + EditCust.activeFuneralHomeName + "%'");
            if (dRows.Length <= 0)
                return "location='" + EditCust.activeFuneralHomeName + "'";

            location = "";
            string str = "";
            string[] Lines = null;
            bool already = false;
            for (int j = 0; j < dRows.Length; j++)
            {
                str = dRows[j]["locations"].ObjToString();
                Lines = str.Split('|');
                for (int i = 0; i < Lines.Length; i++)
                {
                    str = Lines[i].Trim();
                    if (location.Contains(str))
                        continue;
                    if (i > 0 || already)
                        location += " OR ";
                    location += "location='" + Lines[i].Trim() + "'";
                    already = true;
                }
            }
            return location;
        }
        /****************************************************************************************/
        private void gridMain4_MouseDown(object sender, MouseEventArgs e)
        {
            var hitInfo = gridMain4.CalcHitInfo(e.Location);
            if (hitInfo.InRowCell && 1 != 1 )
            {
                int rowHandle = hitInfo.RowHandle;
                rowHandle = gridMain4.FocusedRowHandle;
                dgv4.RefreshDataSource();
                DataTable dt = (DataTable)dgv4.DataSource;

                GridColumn column = hitInfo.Column;
                currentColumn = column.FieldName.Trim();
                if (currentColumn.ToUpper() != "DEPFIRSTNAME" && currentColumn.ToUpper() != "DEPLASTNAME")
                {
                    if (funDemo != null)
                    {
                        if (funDemo.Visible)
                            funDemo.Hide();
                    }
                }
                int row = gridMain4.GetDataSourceRowIndex(rowHandle);
                string data = dt.Rows[row][currentColumn].ObjToString();
                if (data.Trim() == "+")
                {
                    string fullName = dt.Rows[row]["fullName"].ObjToString();
                    data = dt.Rows[row]["depLastName"].ObjToString();
                    if (String.IsNullOrWhiteSpace(data))
                        return;
                    string prefix = dt.Rows[row]["depPrefix"].ObjToString();
                    string suffix = dt.Rows[row]["depSuffix"].ObjToString();
                    string mi = dt.Rows[row]["depMI"].ObjToString();
                    string firstName = dt.Rows[row]["depFirstName"].ObjToString();
                    string lastName = dt.Rows[row]["depLastName"].ObjToString();
                    //string title = dt.Rows[rowHandle]["depPrefix"].ObjToString();
                    //if (!String.IsNullOrWhiteSpace(title))
                    //{
                    //    if (data.IndexOf(title) < 0)
                    //        data = title + " " + data;
                    //}
                    string field = "SRVClergy";
                    DataRow[] dR = trackingDt.Select("tracking='" + field + "'");
                    if (dR.Length > 0)
                    {
                        string u = dR[0]["using"].ObjToString();
                        if (!String.IsNullOrWhiteSpace(u))
                            field = u;
                    }

                    dR = trackDt.Select("tracking='" + field + "' AND answer LIKE '%" + fullName + "%' AND location='" + EditCust.activeFuneralHomeName + "'");
                    if (dR.Length <= 0)
                    {
                        string record = G1.create_record("track", "answer", "-1");
                        if (G1.BadRecord("track", record))
                            return;
                        G1.update_db_table("track", "record", record, new string[] { "tracking", field, "answer", fullName, "location", EditCust.activeFuneralHomeName });
                        G1.update_db_table("track", "record", record, new string[] { "depPrefix", prefix, "depFirstName", firstName, "depMI", mi, "depLastName", lastName, "depSuffix", suffix });
                        DataRow dRow = trackDt.NewRow();
                        dRow["tracking"] = field;
                        dRow["answer"] = data;
                        dRow["record"] = record;
                        trackDt.Rows.Add(dRow);
                    }
                    dt.Rows[row][currentColumn] = "";
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
        private string BuildSelectClergyName(DataTable dt, int rowHandle)
        {
            string prefix = dt.Rows[rowHandle]["depPrefix"].ObjToString();
            string firstName = dt.Rows[rowHandle]["depFirstName"].ObjToString();
            string miName = dt.Rows[rowHandle]["depMI"].ObjToString();
            string lastName = dt.Rows[rowHandle]["depLastName"].ObjToString();
            string suffix = dt.Rows[rowHandle]["depSuffix"].ObjToString();
            string data = prefix.Trim();
            data = "";
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
            //if (columnName.ToUpper() == "DATA")
            //    return;

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
            //if (dgv6.Visible)
            //    return;
            DevExpress.XtraGrid.GridControl dgv = null;
            DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gridMain = null;
            if (whichTab == "MAIN")
            {
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
            }
            else
            {
                if (this.dgv2.Visible)
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
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            MoveRowUp(dt, row);
            //massRowsUp(gridMain, dt, rowHandle);
            dt.AcceptChanges();
            dgv.DataSource = dt;
            gridMain.ClearSelection();
            gridMain.SelectRow(rowHandle - 1);
            gridMain.FocusedRowHandle = rowHandle - 1;
            gridMain.RefreshData();
            dgv.Refresh();
            btnSaveAll.Show();
            if (whichTab == "MAIN")
            {
                if (this.dgvLegal.Visible)
                    FixLegalOrder(dt);
            }
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
            //if (dgv6.Visible)
            //    return;
            DevExpress.XtraGrid.GridControl dgv = null;
            DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gridMain = null;
            if (whichTab == "MAIN")
            {
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
            }
            else
            {
                if (this.dgv2.Visible)
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
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            MoveRowDown(dt, row);
            dt.AcceptChanges();
            dgv.DataSource = dt;
            gridMain.ClearSelection();
            gridMain.SelectRow(rowHandle + 1);
            gridMain.FocusedRowHandle = rowHandle + 1;
            gridMain.RefreshData();
            dgv.Refresh();
            btnSaveAll.Show();
            if (whichTab == "MAIN")
            {
                if (this.dgvLegal.Visible)
                    FixLegalOrder(dt);
            }
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
            DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView grid = GetCurrentGridView();
            DataRow dR = grid.GetFocusedDataRow();
            string column = grid.FocusedColumn.FieldName;
            string str = dR[column].ObjToString();

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
        private void gridMain4_KeyUp(object sender, KeyEventArgs e)
        {
            string column = gridMain4.FocusedColumn.FieldName.Trim().ToUpper();
            if (column != "DEPFIRSTNAME" && column != "DEPLASTNAME")
                return;
            //if (e.KeyCode == Keys.Enter)
            //{
            //    DataRow dr = gridMain4.GetFocusedDataRow();
            //    holdData = dr[column].ObjToString();
            //}
            //else
            //{

            //    savedString += e.KeyCode.ObjToString();
            //    gridMain4.Focus();
            //}
        }
        /****************************************************************************************/
        private string savedString = "";
        private bool cellChanging = false;
        private void gridMainDep_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter )
            {
                if (dgv4.Visible)
                {
                    string columnName = gridMain4.FocusedColumn.FieldName.Trim().ToUpper();
                    if (columnName.ToUpper() == "DEPMI")
                    {
                        DataRow dr = gridMain4.GetFocusedDataRow();
                        string junk = dr["depFirstName"].ObjToString();
                        if (!String.IsNullOrWhiteSpace(junk))
                        {
                            cellChanging = true;
                            return;
                        }
                    }
                }
                if (cellChanging)
                {
                    cellChanging = false;
                    return;
                }

                CheckAddNewRow();
                e.Handled = true;
                cellChanging = false;
                savedString = "";
                return;
            }
            cellChanging = true;
            if (dgv4.Visible)
            {
                if ( e.KeyCode == Keys.Tab )
                {
                }
                //savedString += e.KeyCode.ObjToString();
                string column = gridMain4.FocusedColumn.FieldName.Trim().ToUpper();
                DataTable dt = (DataTable)dgv4.DataSource;
                int row = gridMain4.GetFocusedDataSourceRowIndex();
                //dt.Rows[row][column] = savedString;
                //gridMain4.RefreshData();
                //gridMain4.RefreshEditor(true);
                //dgv4.Refresh();
            }
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
            GridColumn col = gridMainDep.FocusedColumn;
            if ( col.FieldName == "phone")
            {
                int rowHandle = gridMainDep.FocusedRowHandle;
                int row = gridMainDep.GetFocusedDataSourceRowIndex();
                DataRow dr = GetCurrentDataRow();
                if (dr == null)
                    return;
                string phone = dr["phone"].ObjToString();
            }
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
            if (row < 0)
                return;
            //            DataTable dt = (DataTable)dgvDependent.DataSource;
            //DataTable dt = GetCurrentDataTable();
            DataTable dt = (DataTable)dgvDependent.DataSource;
            if (dt == null)
                return;
            dt = (DataTable)dgvLegal.DataSource;
            try
            {
                if (row > dt.Rows.Count - 1)
                    return;

                bool showData = true;
                string delete = dt.Rows[row]["mod"].ObjToString();
                if (delete.ToUpper() == "D")
                    showData = false;
                string relationship = dt.Rows[row]["depRelationship"].ObjToString();
                if (relationship == "PB")
                    showData = false;
                if (relationship == "HPB")
                    showData = false;
                if (relationship == "CLERGY")
                    showData = false;
                if (relationship == "MUSICIAN")
                    showData = false;
                if (relationship == "DISCLOSURES")
                    showData = false;
                if (!showData)
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
        private void repositoryItemCheckEdit7_CheckedChanged(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgvDependent.DataSource;

            //            SaveMembers(dt, "FAM");
            this.Cursor = Cursors.WaitCursor;
            SaveMembers(dt, "");

            DataRow dr = gridMainDep.GetFocusedDataRow();
            string setting = dr["pb"].ObjToString();
            if (setting == "1")
            { // Set, so take it away
                bool mod = btnSaveAll.Visible;
                dr["pb"] = "0";

                string record = dr["record"].ObjToString();
                string mainRecord = record;
                DataRow[] dRows = dt.Select("checkRecord='" + record + "'");
                if (dRows.Length > 0)
                {
                    record = dRows[0]["record"].ObjToString();
                    if (record == "-1")
                        dt.Rows.Remove(dRows[0]);
                    else
                        dRows[0]["mod"] = "D";
                    dgvDependent.DataSource = dt;
                }
                dt = (DataTable)dgv2.DataSource;
                dRows = dt.Select("checkRecord='" + mainRecord + "'");
                if (dRows.Length > 0)
                {
                    record = dRows[0]["record"].ObjToString();
                    if (record == "-1")
                        dt.Rows.Remove(dRows[0]);
                    else
                        dRows[0]["mod"] = "D";
                    dgv2.DataSource = dt;
                    dgv2.RefreshDataSource();
                    dgv2.Refresh();
                }
                funModified = true;
                btnSaveAll.Visible = true;
            }
            else if (setting == "0")
            { // Not Set, so set it up

                string record = dr["record"].ObjToString();
                dr["pb"] = "1";
                bool mod = btnSaveAll.Visible;
                int rowHandle = gridMainDep.FocusedRowHandle;
                int row = gridMainDep.GetDataSourceRowIndex(rowHandle);

                DataTable dx = dt.Clone();

                G1.copy_dt_row(dt, row, dx, 0);
                dx.Rows[0]["record"] = -1;
                dx.Rows[0]["depRelationship"] = "PB";
                dx.Rows[0]["checkRecord"] = record;

                G1.copy_dt_row(dx, 0, dt, dt.Rows.Count);
                dgvDependent.DataSource = dt;

                dt = (DataTable)dgv2.DataSource;
                G1.copy_dt_row(dx, 0, dt, dt.Rows.Count);
                dgv2.DataSource = dt;
                dgv2.RefreshDataSource();
                dgv2.Refresh();


                //SaveMembers(dx, "PB");
                funModified = true;
                btnSaveAll.Visible = true;
            }
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void repositoryItemCheckEdit12_CheckedChanged(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgvDependent.DataSource;

            //SaveMembers(dt, "FAM");
            this.Cursor = Cursors.WaitCursor;
            SaveMembers(dt, "");

            DataRow dr = gridMainDep.GetFocusedDataRow();
            string setting = dr["hpb"].ObjToString();
            if (setting == "1")
            { // Set, so take it away
                bool mod = btnSaveAll.Visible;
                dr["hpb"] = "0";

                string record = dr["record"].ObjToString();
                string mainRecord = record;
                DataRow[] dRows = dt.Select("checkRecord='" + record + "'");
                if (dRows.Length > 0)
                {
                    record = dRows[0]["record"].ObjToString();
                    if (record == "-1")
                        dt.Rows.Remove(dRows[0]);
                    else
                        dRows[0]["mod"] = "D";
                    dgvDependent.DataSource = dt;
                }
                dt = (DataTable)dgv3.DataSource;
                dRows = dt.Select("checkRecord='" + mainRecord + "'");
                if (dRows.Length > 0)
                {
                    record = dRows[0]["record"].ObjToString();
                    if (record == "-1")
                        dt.Rows.Remove(dRows[0]);
                    else
                        dRows[0]["mod"] = "D";
                    dgv3.DataSource = dt;
                    dgv3.RefreshDataSource();
                    dgv3.Refresh();
                }
                funModified = true;
                btnSaveAll.Visible = true;
            }
            else if (setting == "0")
            { // Not Set, so set it up

                string record = dr["record"].ObjToString();
                dr["hpb"] = "1";
                bool mod = btnSaveAll.Visible;
                int rowHandle = gridMainDep.FocusedRowHandle;
                int row = gridMainDep.GetDataSourceRowIndex(rowHandle);

                DataTable dx = dt.Clone();

                G1.copy_dt_row(dt, row, dx, 0);
                dx.Rows[0]["record"] = -1;
                dx.Rows[0]["depRelationship"] = "HPB";
                dx.Rows[0]["checkRecord"] = record;

                G1.copy_dt_row(dx, 0, dt, dt.Rows.Count);
                dgvDependent.DataSource = dt;

                dt = (DataTable)dgv3.DataSource;
                G1.copy_dt_row(dx, 0, dt, dt.Rows.Count);
                dgv3.DataSource = dt;
                dgv3.RefreshDataSource();
                dgv3.Refresh();


                //SaveMembers(dx, "PB");
                funModified = true;
                btnSaveAll.Visible = true;
            }
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void DoPallBearers(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgvDependent.DataSource;
            DataRow dr = gridMainDep.GetFocusedDataRow();
            string setting = dr["pb"].ObjToString();
            if (setting == "1")
            { // Set, so take it away
                bool mod = btnSaveAll.Visible;
                dr["pbCheck"] = "0";
                funModified = mod;
                btnSaveAll.Visible = mod;
            }
            else if (setting == "0")
            { // Not Set, so set it up
                bool mod = btnSaveAll.Visible;
                dr["pbCheck"] = "1";
                funModified = true;
                btnSaveAll.Visible = true;
                DataTable tempDt = (DataTable)dgv2.DataSource;
            }
        }
        /****************************************************************************************/
        private void DoHonoraryPallBearers(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgvDependent.DataSource;
            DataRow dr = gridMainDep.GetFocusedDataRow();
            string setting = dr["hpb"].ObjToString();
            if (setting == "1")
            { // Set, so take it away
                bool mod = btnSaveAll.Visible;
                dr["hpbCheck"] = "0";
                funModified = mod;
                btnSaveAll.Visible = mod;
            }
            else if (setting == "0")
            { // Not Set, so set it up
                bool mod = btnSaveAll.Visible;
                dr["hpbCheck"] = "1";
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
            //for ( int i=0; i<dt.Rows.Count; i++)
            //{
            //    dt.Rows[i]["pb"] = "0";
            //    dt.Rows[i]["hpb"] = "0";
            //}
            //string cmd = "Select * from `relatives` where `contractNumber` = '" + workContract + "' AND (`depRelationship` = 'PB' OR `depRelationship` = 'HPB');";
            //DataTable dx = G1.get_db_data(cmd);
            //for ( int i=0; i<dx.Rows.Count; i++)
            //{
            //    fname = dx.Rows[i]["depFirstName"].ObjToString();
            //    lname = dx.Rows[i]["depLastName"].ObjToString();
            //    mi = dx.Rows[i]["depMI"].ObjToString();
            //    relation = dx.Rows[i]["depRelationship"].ObjToString();
            //    try
            //    {
            //        dR = dt.Select("depFirstName='" + fname + "' AND depLastName='" + lname + "' AND depMI='" + mi + "'");
            //        if (dR.Length > 0)
            //        {
            //            if (relation.ToUpper() == "HPB")
            //            {
            //                dR[0]["hpb"] = "1";
            //                dR[0]["hpbCheck"] = "1";
            //            }
            //            else if (relation.ToUpper() == "PB")
            //            {
            //                dR[0]["pb"] = "1";
            //                dR[0]["pbCheck"] = "1";
            //            }
            //        }
            //    }
            //    catch ( Exception ex)
            //    {
            //    }
            //}
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
            //MergePallBearers();
        }
        /****************************************************************************************/
        private void dgv3_VisibleChanged(object sender, EventArgs e)
        {
            //MergeHPB();
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
        private void gridMain2_CustomRowFilter(object sender, DevExpress.XtraGrid.Views.Base.RowFilterEventArgs e)
        {
            int row = e.ListSourceRow;
            DataTable dt = (DataTable)dgv2.DataSource;
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
                //string pbCheck = dt.Rows[row]["pbCheck"].ObjToString();
                //if (pbCheck == "1")
                //    return;
                string relation = dt.Rows[row]["depRelationship"].ObjToString().ToUpper();
                if (relation != "PB")
                {
                    e.Visible = false;
                    e.Handled = true;
                    return;
                }
            }
            catch (Exception ex)
            {
                // MessageBox.Show("***ERROR*** Not Showing Deleted Members for Contract " + workContract + " Error " + ex.Message.ToString());
            }
        }
        /****************************************************************************************/
        private void gridMain3_CustomRowFilter(object sender, DevExpress.XtraGrid.Views.Base.RowFilterEventArgs e)
        {
            int row = e.ListSourceRow;
            DataTable dt = (DataTable)dgv3.DataSource;
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
                //string hpbCheck = dt.Rows[row]["hpbCheck"].ObjToString();
                //if (hpbCheck == "1")
                //    return;
                string relation = dt.Rows[row]["depRelationship"].ObjToString().ToUpper();
                if (relation != "HPB")
                {
                    e.Visible = false;
                    e.Handled = true;
                    return;
                }
            }
            catch (Exception ex)
            {
                // MessageBox.Show("***ERROR*** Not Showing Deleted Members for Contract " + workContract + " Error " + ex.Message.ToString());
            }
        }
        /***************************************************************************************/
        public delegate void d_void_FamModified(DataTable dt);
        public event d_void_FamModified FamModified;
        protected void OnFamModified()
        {
            DataTable dt = (DataTable)dgvDependent.DataSource;
            FamModified?.Invoke(dt);
        }
        /***************************************************************************************/
        public delegate void d_void_PallModified(DataTable dt);
        public event d_void_PallModified PallModified;
        protected void OnPallModified()
        {
            DataTable dt = (DataTable)dgv2.DataSource;
            if (PallModified != null)
                PallModified?.Invoke(dt);
            else if (SomethingChanged != null)
                SomethingChanged?.Invoke("PB");
        }
        /***************************************************************************************/
        public delegate void d_void_HPallModified(DataTable dt);
        public event d_void_HPallModified HPallModified;
        protected void OnHPallModified()
        {
            DataTable dt = (DataTable)dgv3.DataSource;
            if (HPallModified != null)
                HPallModified?.Invoke(dt);
            else if (SomethingChanged != null)
                SomethingChanged?.Invoke("HPB");
        }
        /***************************************************************************************/
        public delegate void d_void_SomethingChanged(string what);
        public event d_void_SomethingChanged SomethingChanged;
        protected void OnSomethingChanged(string what)
        {
            SomethingChanged?.Invoke(what);
        }
        /***************************************************************************************/
        public void FireEventFamModified(DataTable masterDt)
        {
            DataRow dR1 = null;
            DataRow dR2 = null;

            DataRow[] dR = null;

            masterDt = (DataTable)dgvDependent.DataSource;

            DataTable dt = (DataTable)dgv2.DataSource;
            if (dt != null)
            {
                bool pbModified = false;


                DataRow[] dRows = masterDt.Select("checkRecord<>''");
                if (dRows.Length > 0)
                {
                    string record = "";
                    string isChecked = "";
                    string relation = "";
                    int order = 0;
                    string pbRecord = "";
                    string pbCheck = "";
                    string checkRecord = "";
                    for (int i = 0; i < dRows.Length; i++) // DataRows of Pall Bearers
                    {
                        relation = dRows[i]["depRelationship"].ObjToString();
                        if (relation != "PB")
                            continue;
                        record = dRows[i]["record"].ObjToString();
                        checkRecord = dRows[i]["checkRecord"].ObjToString();
                        if (String.IsNullOrWhiteSpace(checkRecord)) // Must be new
                            continue;
                        order = dRows[i]["PalOrder"].ObjToInt32();
                        dR = masterDt.Select("record='" + checkRecord + "'"); // See if Family Member is also a Pall Bearer
                        if (dR.Length > 0) //Got Someone who is a Pall Bearer and Family Member
                        {
                            dR2 = dRows[i]; // Pall Bearer DataRow
                            dR1 = dR[0]; // Master DataRow of Family Member

                            G1.copy_dr_row(dR1, dR2);

                            dR2["record"] = record;
                            dR2["PalOrder"] = order;
                            dR2["checkRecord"] = checkRecord;
                            dR2["depRelationship"] = "PB";
                            pbModified = true;

                            if (record != "-1")
                            {
                                dR = dt.Select("checkRecord='" + checkRecord + "' AND mod <> 'D'"); // Now replace actual Pall Bearer in DataGrid
                                if (dR.Length > 0) //Got Someone who is a Pall Bearer and Family Member
                                {
                                    dR2 = dR[0]; // Pall Bearer DataRow

                                    G1.copy_dr_row(dR1, dR2);

                                    dR2["record"] = record;
                                    dR2["PalOrder"] = order;
                                    dR2["checkRecord"] = checkRecord;
                                    dR2["depRelationship"] = "PB";
                                }
                            }
                            else // Must be a new one
                            {
                                dR = dt.Select("checkRecord='" + checkRecord + "' AND mod <> 'D'");
                                if (dR.Length <= 0)
                                {
                                    DataRow newRow = dt.NewRow();
                                    try
                                    {
                                        dt.Rows.Add(newRow);
                                        int row = dt.Rows.Count - 1;
                                        G1.copy_dr_row(dR2, dt.Rows[row]);
                                        dt.Rows[row]["checkRecord"] = checkRecord;
                                        dt.Rows[row]["depRelationship"] = "PB";
                                        dt.Rows[row]["palOrder"] = 0;
                                    }
                                    catch (Exception ex)
                                    {
                                    }
                                }
                                else
                                {
                                    dR2 = dR[0]; // Pall Bearer DataRow

                                    G1.copy_dr_row(dR1, dR2);

                                    dR2["record"] = record;
                                    dR2["Order"] = order;
                                    dR2["checkRecord"] = checkRecord;
                                    dR2["depRelationship"] = "PB";
                                }
                            }
                        }
                    }
                    dRows = dt.Select("checkRecord<>''");
                    if (dRows.Length > 0)
                    {
                        bool gotOne = false;
                        for (int i = 0; i < dRows.Length; i++)
                        {
                            record = dRows[i]["checkRecord"].ObjToString();
                            dR = masterDt.Select("record = '" + record + "'");
                            if (dR.Length > 0)
                            {
                                if (dR[0]["PB"].ObjToString() != "1" || dR[0]["mod"].ObjToString().ToUpper() == "D")
                                {
                                    dRows[i]["checkRecord"] = "-1";
                                    gotOne = true;
                                }
                            }
                        }
                        if (gotOne)
                        {
                            for (int i = dt.Rows.Count - 1; i >= 0; i--)
                            {
                                record = dt.Rows[i]["checkRecord"].ObjToString();
                                if (record == "-1")
                                    dt.Rows.RemoveAt(i);
                            }
                        }
                    }
                    if (pbModified)
                    {
                        dgv2.DataSource = dt;
                        dgv2.RefreshDataSource();
                        dgv2.Refresh();
                        funModified = true;
                        btnSaveAll.Show();
                        btnSaveAll.Refresh();
                    }
                }
            }


            dt = (DataTable)dgv3.DataSource;
            if (dt != null)
            {
                bool pbModified = false;


                DataRow[] dRows = masterDt.Select("checkRecord<>''");
                if (dRows.Length > 0)
                {
                    string record = "";
                    string isChecked = "";
                    string relation = "";
                    int order = 0;
                    string pbRecord = "";
                    string pbCheck = "";
                    string checkRecord = "";
                    for (int i = 0; i < dRows.Length; i++) // DataRows of HPB's
                    {
                        relation = dRows[i]["depRelationship"].ObjToString();
                        if (relation != "HPB")
                            continue;
                        record = dRows[i]["record"].ObjToString();
                        checkRecord = dRows[i]["checkRecord"].ObjToString();
                        if (String.IsNullOrWhiteSpace(checkRecord)) // Must be new
                            continue;
                        order = dRows[i]["Order"].ObjToInt32();
                        dR = masterDt.Select("record='" + checkRecord + "'"); // See if Family Member is also an HPB
                        if (dR.Length > 0) //Got Someone who is a Pall Bearer and Family Member
                        {
                            dR2 = dRows[i]; // HPB DataRow
                            dR1 = dR[0]; // Master DataRow of Family Member

                            G1.copy_dr_row(dR1, dR2);

                            dR2["record"] = record;
                            dR2["Order"] = order;
                            dR2["checkRecord"] = checkRecord;
                            dR2["depRelationship"] = "HPB";
                            pbModified = true;

                            if (record != "-1")
                            {
                                dR = dt.Select("checkRecord='" + checkRecord + "' AND mod <> 'D'"); // Now replace actual Pall Bearer in DataGrid
                                if (dR.Length > 0) //Got Someone who is a Pall Bearer and Family Member
                                {
                                    dR2 = dR[0]; // Pall Bearer DataRow

                                    G1.copy_dr_row(dR1, dR2);

                                    dR2["record"] = record;
                                    dR2["Order"] = order;
                                    dR2["checkRecord"] = checkRecord;
                                    dR2["depRelationship"] = "HPB";
                                }
                            }
                            else
                            {
                                dR = dt.Select("checkRecord='" + checkRecord + "' AND mod <> 'D' ");
                                if (dR.Length <= 0)
                                {
                                    DataRow newRow = dt.NewRow();
                                    try
                                    {
                                        dt.Rows.Add(newRow);
                                        int row = dt.Rows.Count - 1;
                                        G1.copy_dr_row(dR2, dt.Rows[row]);
                                        dt.Rows[row]["checkRecord"] = checkRecord;
                                        dt.Rows[row]["depRelationship"] = "HPB";
                                        dt.Rows[row]["palOrder"] = 0;
                                    }
                                    catch (Exception ex)
                                    {
                                    }
                                }
                                else
                                {
                                    dR2 = dR[0]; // Pall Bearer DataRow

                                    G1.copy_dr_row(dR1, dR2);

                                    dR2["record"] = record;
                                    dR2["Order"] = order;
                                    dR2["checkRecord"] = checkRecord;
                                    dR2["depRelationship"] = "HPB";
                                }
                            }
                        }
                    }
                    dRows = dt.Select("checkRecord<>''");
                    if (dRows.Length > 0)
                    {
                        bool gotOne = false;
                        for (int i = 0; i < dRows.Length; i++)
                        {
                            record = dRows[i]["checkRecord"].ObjToString();
                            dR = masterDt.Select("record = '" + record + "'");
                            if (dR.Length > 0)
                            {
                                if (dR[0]["HPB"].ObjToString() != "1" || dR[0]["mod"].ObjToString().ToUpper() == "D")
                                {
                                    dRows[i]["checkRecord"] = "-1";
                                    gotOne = true;
                                }
                            }
                        }
                        if (gotOne)
                        {
                            for (int i = dt.Rows.Count - 1; i >= 0; i--)
                            {
                                record = dt.Rows[i]["checkRecord"].ObjToString();
                                if (record == "-1")
                                    dt.Rows.RemoveAt(i);
                            }
                        }
                    }
                    if (pbModified)
                    {
                        dgv3.DataSource = dt;
                        dgv3.RefreshDataSource();
                        dgv3.Refresh();
                        funModified = true;
                        btnSaveAll.Show();
                        btnSaveAll.Refresh();
                    }
                }
            }
        }
        /***************************************************************************************/
        public void FireEventPallModified(DataTable masterDt)
        {
            DataTable dt = (DataTable)dgvDependent.DataSource;
            if (dt == null)
                return;

            if (masterDt == null)
                return;

            DataRow dR1 = null;
            DataRow dR2 = null;

            DataRow[] dR = null;

            string record = "";
            string checkRecord = "";
            string relation = "";
            bool pModified = false;

            DataRow[] dRows = masterDt.Select("checkRecord<>''");

            if (dRows.Length <= 0)
                return;

            DataTable testDt = dRows.CopyToDataTable();

            for (int i = 0; i < dRows.Length; i++)
            {
                checkRecord = dRows[i]["checkRecord"].ObjToString();
                dR = dt.Select("record='" + checkRecord + "'");
                if (dR.Length > 0)
                {
                    testDt = dR.CopyToDataTable();

                    relation = dR[0]["depRelationship"].ObjToString();

                    dR1 = dRows[i]; // Pall Bearer in dgv2
                    dR2 = dR[0];
                    G1.copy_dr_row(dR1, dR2);

                    dR2["depRelationship"] = relation;
                    dR2["record"] = checkRecord;
                    dR2["pb"] = "1";
                    dR2["checkRecord"] = "";
                    pModified = true;
                }
            }
            if (pModified)
            {
                dgvDependent.DataSource = dt;
                dgvDependent.RefreshDataSource();
                dgvDependent.Refresh();
                funModified = true;
                btnSaveAll.Show();
                btnSaveAll.Refresh();
            }
        }
        /***************************************************************************************/
        public void FireEventHPallModified(DataTable masterDt)
        {
            DataTable dt = (DataTable)dgvDependent.DataSource;
            if (dt == null)
                return;

            if (masterDt == null)
                return;

            DataRow dR1 = null;
            DataRow dR2 = null;

            DataRow[] dR = null;

            string record = "";
            string checkRecord = "";
            string relation = "";
            bool pModified = false;

            DataRow[] dRows = masterDt.Select("checkRecord<>''");

            if (dRows.Length <= 0)
                return;

            DataTable testDt = dRows.CopyToDataTable();

            for (int i = 0; i < dRows.Length; i++)
            {
                checkRecord = dRows[i]["checkRecord"].ObjToString();
                dR = dt.Select("record='" + checkRecord + "'");
                if (dR.Length > 0)
                {
                    testDt = dR.CopyToDataTable();

                    relation = dR[0]["depRelationship"].ObjToString();

                    dR1 = dRows[i]; // HPB in dgv3
                    dR2 = dR[0];
                    G1.copy_dr_row(dR1, dR2);

                    dR2["depRelationship"] = relation;
                    dR2["record"] = checkRecord;
                    dR2["hpb"] = "1";
                    dR2["checkRecord"] = "";
                    pModified = true;
                }
            }
            if (pModified)
            {
                dgvDependent.DataSource = dt;
                dgvDependent.RefreshDataSource();
                dgvDependent.Refresh();
                funModified = true;
                btnSaveAll.Show();
                btnSaveAll.Refresh();
            }
        }
        /***************************************************************************************/
        public void FireEventFamModifiedx(DataTable masterDt)
        {
            DataRow dR1 = null;
            DataRow dR2 = null;

            DataRow[] dR = null;

            DataTable dt = (DataTable)dgv2.DataSource;
            if (dt != null)
            {
                bool pbModified = false;

                string checkRecord = "";

                DataRow[] dRows = masterDt.Select("pb='1'");
                if (dRows.Length > 0)
                {
                    string record = "";
                    string isChecked = "";
                    string relation = "";
                    int order = 0;
                    string pbRecord = "";
                    string pbCheck = "";
                    for (int i = 0; i < dRows.Length; i++) // DataRows of Pall Bearers
                    {
                        record = dRows[i]["record"].ObjToString();
                        if (String.IsNullOrWhiteSpace(record)) // Must be new
                            continue;
                        dR = dt.Select("checkRecord='" + record + "'"); // See if Family Member is also a Pall Bearer
                        if (dR.Length > 0) //Got Someone who is a Pall Bearer and Family Member
                        {
                            pbRecord = dR[0]["record"].ObjToString();
                            order = dR[0]["PalOrder"].ObjToInt32();

                            dR1 = dRows[0];
                            dR2 = dR[0];

                            G1.copy_dr_row(dR1, dR2);

                            dR2["record"] = pbRecord;
                            dR2["PalOrder"] = order;
                            dR2["checkRecord"] = record;
                            dR2["depRelationship"] = "PB";
                            pbModified = true;
                        }
                    }
                    if (pbModified)
                    {
                        dgv2.DataSource = dt;
                        dgv2.RefreshDataSource();
                        dgv2.Refresh();
                    }
                }
            }

            dt = (DataTable)dgv3.DataSource;
            if (dt != null)
            {
                bool pbModified = false;

                DataRow[] dRows = masterDt.Select("hpb='1'");
                if (dRows.Length > 0)
                {
                    string record = "";
                    string isChecked = "";
                    string relation = "";
                    int order = 0;
                    string pbRecord = "";
                    string pbCheck = "";
                    for (int i = 0; i < dRows.Length; i++) // DataRows of Pall Bearers
                    {
                        record = dRows[i]["record"].ObjToString();
                        if (String.IsNullOrWhiteSpace(record)) // Must be new
                            continue;
                        dR = dt.Select("checkRecord='" + record + "'"); // See if Family Member is also a Pall Bearer
                        if (dR.Length > 0) //Got Someone who is a Pall Bearer and Family Member
                        {
                            pbRecord = dR[0]["record"].ObjToString();

                            dR1 = dRows[0];
                            dR2 = dR[0];

                            G1.copy_dr_row(dR1, dR2);

                            dR2["record"] = pbRecord;
                            dR2["checkRecord"] = record;
                            dR2["depRelationship"] = "HPB";
                            pbModified = true;
                        }
                    }
                    if (pbModified)
                    {
                        dgv3.DataSource = dt;
                        dgv3.RefreshDataSource();
                        dgv3.Refresh();
                    }
                }
            }
        }
        /****************************************************************************************/
        public int g1 = 0;
        public int g2 = 0;
        public int g3 = 0;
        public int g4 = 0;
        public int g5 = 0;
        public int g6 = 0;
        public int g7 = 0;
        public int g8 = 0;
        private void tabControl1_Enter(object sender, EventArgs e)
        {
            if (whichTab == "MAIN")
            {
                if (dgvDependent.Visible)
                {
                    gridMainDep.ClearSelection();
                    gridMainDep.SelectRow(g1);
                    gridMainDep.FocusedRowHandle = g1;
                    gridMainDep.RefreshData();
                    dgvDependent.Refresh();
                }

                if (dgvLegal.Visible)
                {
                    gridMainLegal.ClearSelection();
                    gridMainLegal.SelectRow(g2);
                    gridMainLegal.FocusedRowHandle = g2;
                    gridMainLegal.RefreshData();
                    dgvLegal.Refresh();
                }
            }
            else
            {
                if (dgv6.Visible)
                {
                    gridMain6.ClearSelection();
                    gridMain6.FocusedRowHandle = g3;
                    gridMain6.SelectRow(g3);
                    //gridMain6.RefreshData();
                    //gridMain6.RefreshEditor(true);
                    dgv6.RefreshDataSource();
                    dgv6.Refresh();
                    gridMain6.ExpandAllGroups();
                }

                if (dgv2.Visible)
                {
                    gridMain2.ClearSelection();
                    gridMain2.SelectRow(g4);
                    gridMain2.FocusedRowHandle = g4;
                    gridMain2.RefreshData();
                    dgv2.Refresh();
                }

                if (dgv3.Visible)
                {
                    gridMain3.ClearSelection();
                    gridMain3.SelectRow(g5);
                    gridMain3.FocusedRowHandle = g5;
                    gridMain3.RefreshData();
                    dgv3.Refresh();
                }
            }

            //            gridMain6.ExpandAllGroups();
        }
        /****************************************************************************************/
        private void tabControl1_Leave(object sender, EventArgs e)
        {
            if (whichTab == "MAIN")
            {
                if (dgvDependent.Visible)
                {
                    gridMainDep.ClearSelection();
                    gridMainDep.SelectRow(g1);
                    gridMainDep.FocusedRowHandle = g1;
                    gridMainDep.RefreshData();
                    dgvDependent.Refresh();
                }

                if (dgvLegal.Visible)
                {
                    gridMainLegal.ClearSelection();
                    gridMainLegal.SelectRow(g2);
                    gridMainLegal.FocusedRowHandle = g2;
                    gridMainLegal.RefreshData();
                    dgvLegal.Refresh();
                }
            }
            else
            {
                if (dgv6.Visible)
                {
                    gridMain6.ClearSelection();
                    gridMain6.FocusedRowHandle = g3;
                    gridMain6.SelectRow(g3);
                    //gridMain6.RefreshData();
                    dgv6.RefreshDataSource();
                    dgv6.Refresh();
                    gridMain6.ExpandAllGroups();
                }

                if (dgv2.Visible)
                {
                    gridMain2.ClearSelection();
                    gridMain2.SelectRow(g4);
                    gridMain2.FocusedRowHandle = g4;
                    gridMain2.RefreshData();
                    dgv2.Refresh();
                }

                if (dgv3.Visible)
                {
                    gridMain3.ClearSelection();
                    gridMain3.SelectRow(g5);
                    gridMain3.FocusedRowHandle = g5;
                    gridMain3.RefreshData();
                    dgv3.Refresh();
                }
            }

            //            gridMain6.ExpandAllGroups();
        }
        /****************************************************************************************/
        private void dgvDependent_Leave(object sender, EventArgs e)
        {
            g1 = gridMainDep.FocusedRowHandle;
        }
        /****************************************************************************************/
        private void dgvLegal_Leave(object sender, EventArgs e)
        {
            g2 = gridMainLegal.FocusedRowHandle;
        }
        /****************************************************************************************/
        private void dgv6_Leave(object sender, EventArgs e)
        {
            g3 = gridMain6.FocusedRowHandle;
        }
        /****************************************************************************************/
        private void dgv2_Leave(object sender, EventArgs e)
        {
            g4 = gridMain2.FocusedRowHandle;
        }
        /****************************************************************************************/
        private void dgv3_Leave(object sender, EventArgs e)
        {
            g5 = gridMain3.FocusedRowHandle;
        }
        /****************************************************************************************/
        private void dgvDependent_Enter(object sender, EventArgs e)
        {
            gridMainDep.ClearSelection();
            gridMainDep.SelectRow(g1);
            gridMainDep.FocusedRowHandle = g1;
            gridMainDep.RefreshData();
            dgvDependent.Refresh();
        }
        /****************************************************************************************/
        private void dgvLegal_Enter(object sender, EventArgs e)
        {
            gridMainLegal.ClearSelection();
            gridMainLegal.SelectRow(g2);
            gridMainLegal.FocusedRowHandle = g2;
            //gridMainLegal.RefreshData();
            //dgvLegal.Refresh();
        }
        /****************************************************************************************/
        private void dgv6_Enter(object sender, EventArgs e)
        {
            gridMain6.ClearSelection();
            gridMain6.SelectRow(g3);
            gridMain6.FocusedRowHandle = g3;
            //gridMain6.RefreshData();
            //dgv6.Refresh();
        }
        /****************************************************************************************/
        private void dgv2_Enter(object sender, EventArgs e)
        {
            gridMain2.ClearSelection();
            gridMain2.SelectRow(g4);
            gridMain2.FocusedRowHandle = g4;
            gridMain2.RefreshData();
            dgv2.Refresh();
        }
        /****************************************************************************************/
        private void dgv3_Enter(object sender, EventArgs e)
        {
            gridMain3.ClearSelection();
            gridMain3.SelectRow(g5);
            gridMain3.FocusedRowHandle = g5;
            gridMain3.RefreshData();
            dgv3.Refresh();
        }
        /***************************************************************************************/
        public void FireEventServiceDateChanged()
        {
            string cmd = "Select * from `fcust_extended` where `contractNumber` = '" + workContract + "';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count > 0)
            {
                DateTime date = dx.Rows[0]["serviceDate"].ObjToDateTime();
                DataTable dt = (DataTable)dgv6.DataSource;
                if (dt.Rows.Count > 0)
                {
                    DataRow[] dRows = dt.Select("dbField='SRVDATE'");
                    if (dRows.Length > 0)
                    {
                        dRows[0]["data"] = date.ToString("MM/dd/yyyy");
                        gridMain6.RefreshEditor(true);
                        dgv6.Refresh();
                    }
                }
            }
        }
        /****************************************************************************************/
        private void gridMain4_DoubleClick(object sender, EventArgs e)
        {
            DataRow dr = gridMain4.GetFocusedDataRow();
            if (dr == null)
                return;
            DataTable dt = (DataTable)dgv4.DataSource;
            int rowHandle = gridMain4.FocusedRowHandle;
            int row = gridMain4.GetDataSourceRowIndex(rowHandle);
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
                string prefix = "";
                string firstName = "";
                string lastName = "";
                string mi = "";
                string suffix = "";
                G1.ParseOutName(newAnswer, ref prefix, ref firstName, ref lastName, ref mi, ref suffix);
                dt.Rows[row]["depPrefix"] = prefix;
                dt.Rows[row]["depFirstName"] = firstName;
                dt.Rows[row]["depLastName"] = lastName;
                dt.Rows[row]["depMI"] = mi;
                dt.Rows[row]["depSuffix"] = suffix;
                dt.Rows[row]["fullName"] = newAnswer;
                dt.Rows[row]["add"] = "";
                string cmd = "Select * from `track` where `tracking` = '" + field + "' and `location` = '" + EditCust.activeFuneralHomeName + "' AND `answer` = '" + newAnswer + "';";
                DataTable dx = G1.get_db_data(cmd);
                if (dx.Rows.Count <= 0)
                {
                    dt.Rows[rowHandle]["add"] = "+";
                    dt.Rows[rowHandle]["edit"] = "E";
                }
                loading = false;
                dgv4.RefreshDataSource();
                FireEventFunServicesSetModified();
            }
        }
        /****************************************************************************************/
        private int activeColumnIndex = -1;
        private string cellValue = "";
        private void gridMain4_CellValueChanging(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            DataTable dt = (DataTable)dgv4.DataSource;
            activeColumnIndex = e.Column.VisibleIndex;
            string columnName = e.Column.FieldName.ObjToString();
            cellValue = e.Value.ObjToString();
            int rowHandle = gridMain4.FocusedRowHandle;
            int row = gridMain4.GetDataSourceRowIndex(rowHandle);
            DataRow dr = gridMain4.GetFocusedDataRow();

            if (String.IsNullOrWhiteSpace(cellValue))
            {
              if (columnName.ToUpper() == "DEPMI")
                {
                    string junk = dr["depFirstName"].ObjToString();
                    if (!String.IsNullOrWhiteSpace(junk))
                    {
                        cellChanging = true;
                        return;
                    }
                }
                cellChanging = false;
                return;
            }

            if (columnName.ToUpper() == "DEPFIRSTNAME")
            {
                cellChanging = true;
                object junk = gridMain4.GetRowCellValue(gridMain4.FocusedRowHandle, gridMain4.FocusedColumn);
                object junk1 = gridMain4.GetRowCellDisplayText(gridMain4.FocusedRowHandle, gridMain4.FocusedColumn);

            }
            else if (columnName.ToUpper() == "DEPLASTNAME")
            {
                cellChanging = true;
            }
            else if (columnName.ToUpper() == "DEPMI")
            {
                cellChanging = true;
            }
            else if (columnName.ToUpper() == "DEPPREFIX")
            {
                cellChanging = true;
            }
            else if (columnName.ToUpper() == "DEPSUFFIX")
            {
                cellChanging = true;
            }

            if (1 == 1)
                return;
            //FindClergyData(columnName, cellValue, row);
        }
        /****************************************************************************************/
        private void FindClergyData(string column, string what, int row)
        {
            if (String.IsNullOrWhiteSpace(column) || String.IsNullOrWhiteSpace(what))
                return;
            column = column.ToUpper();

            string saveWhat = what;
            string saveColumn = column;

            if (column != "DEPPREFIX" && column != "DEPSUFFIX" && column != "DEPMI" && column != "DEPFIRSTNAME" && column != "DEPLASTNAME")
                return;

            //column = column.Replace("DEP", "");

            DataTable cDt = null;
            string locations = "";
            if (!String.IsNullOrWhiteSpace(EditCust.activeFuneralHomeName))
                locations = findLocationAssociation();

            string cmd = "";
            if (!String.IsNullOrWhiteSpace(locations))
            {
                try
                {
                    cmd = "Select * from `track` WHERE `tracking` = 'SRVClergy' AND (" + locations + ") AND ( `" + column + "` LIKE '%" + what + "%' );";
                    cDt = G1.get_db_data(cmd);
                    if (cDt.Rows.Count <= 0)
                    {
                        cmd = "Select * from `track` WHERE `tracking` = 'SRVClergy' AND (" + locations + ") AND ( `answer` LIKE '%" + what + "%' )";
                        cDt = G1.get_db_data(cmd);
                    }
                }
                catch (Exception ex)
                {
                }
            }
            else
            {
                try
                {
                    cmd = "Select * from `track` WHERE `tracking` = 'SRVClergy' AND ( `" + column + "` LIKE '%" + what + "%' );";
                    cDt = G1.get_db_data(cmd);
                    if (cDt.Rows.Count <= 0)
                    {
                        cmd = "Select * from `track` WHERE `tracking` = 'SRVClergy' AND ( `answer` LIKE '%" + what + "%' )";
                        cDt = G1.get_db_data(cmd);
                    }
                }
                catch (Exception ex)
                {
                }
            }
            if (cDt.Rows.Count <= 0)
                return;

            DataRow dr = gridMain4.GetFocusedDataRow();
            if (dr == null)
                return;

            cDt = ReBuildClergyTable(cDt, column);

            what = cDt.Rows[0]["answer"].ObjToString();
            string prefix = cDt.Rows[0]["prefix"].ObjToString();
            string firstName = cDt.Rows[0]["firstname"].ObjToString();
            string lastName = cDt.Rows[0]["lastname"].ObjToString();
            string mi = cDt.Rows[0]["middlename"].ObjToString();
            string suffix = cDt.Rows[0]["suffix"].ObjToString();
            if (String.IsNullOrWhiteSpace(lastName))
                G1.ParseOutName(what, ref prefix, ref firstName, ref lastName, ref mi, ref suffix);

            if (column != "PREFIX")
                dr["depPrefix"] = prefix;
            if (column != "FIRSTNAME")
                dr["depFirstName"] = firstName;
            if (column != "LASTNAME")
                dr["depLastName"] = lastName;
            if (column != "MIDDLENAME")
                dr["depMI"] = mi;
            if (column != "SUFFIX")
                dr["depSuffix"] = suffix;

            dr[saveColumn] = saveWhat;

            dr["fullName"] = what;

            gridMain4.RefreshEditor(true);
        }
        /****************************************************************************************/
        private DataTable ReBuildClergyTable(DataTable dt, string columName)
        {
            if (dt.Rows.Count <= 0)
                return dt;
            if (String.IsNullOrWhiteSpace(columName))
                return dt;

            string what = "";
            string prefix = "";
            string firstName = "";
            string lastName = "";
            string mi = "";
            string suffix = "";

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                what = dt.Rows[i]["answer"].ObjToString();
                prefix = dt.Rows[i]["prefix"].ObjToString();
                firstName = dt.Rows[i]["firstname"].ObjToString();
                lastName = dt.Rows[i]["lastname"].ObjToString();
                mi = dt.Rows[i]["middlename"].ObjToString();
                suffix = dt.Rows[i]["suffix"].ObjToString();
                if (String.IsNullOrWhiteSpace(lastName))
                {
                    G1.ParseOutName(what, ref prefix, ref firstName, ref lastName, ref mi, ref suffix);
                    dt.Rows[i]["prefix"] = prefix;
                    dt.Rows[i]["firstname"] = firstName;
                    dt.Rows[i]["lastname"] = lastName;
                    dt.Rows[i]["middlename"] = mi;
                    dt.Rows[i]["suffix"] = suffix;
                }
            }

            DataView tempview = dt.DefaultView;
            tempview.Sort = columName + " asc";
            dt = tempview.ToTable();

            return dt;
        }
        /****************************************************************************************/
        private void gridMain6_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter || e.KeyData == Keys.Tab || e.KeyData == Keys.Up || e.KeyData == Keys.Down )
            {
                try
                {
                    DataTable dt = (DataTable)dgv6.DataSource;
                    int rowHandle = gridMain6.FocusedRowHandle;

                    int row = gridMain6.FocusedRowHandle;
                    if (row >= 0)
                    {
                        string str = dt.Rows[row]["dbfield"].ObjToString();
                        GridColumn currCol = gridMain6.FocusedColumn;
                        string currentColumn = currCol.FieldName;
                        if ( e.KeyData == Keys.Up )
                            rowHandle--;
                        else
                            rowHandle++;

                        if (currentColumn.ToUpper() == "DATA")
                        {
                            if (rowHandle > (dt.Rows.Count - 1))
                            {
                                gridMain6.FocusedColumn = gridMain6.Columns["data"];
                                rowHandle = 0;
                            }
                            else if ( rowHandle < 0 )
                            {
                                gridMain6.FocusedColumn = gridMain6.Columns["data"];
                                rowHandle = 0;
                            }
                        }
                        //gridMain6.SelectRow(rowHandle);
                        gridMain6.FocusedRowHandle = rowHandle;

                        e.Handled = true;
                        holdKey = "";
                        gridMain6_ShownEditor(null, null);
                    }
                }
                catch ( Exception ex )
                {
                    MessageBox.Show("***ERROR*** Data Error.\n" + ex.Message.ToString(), "Data Entry Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                }
            }
            else
            {
                try
                {
                    e.Handled = false;
                    holdKey = e.KeyData.ObjToString();
                    if ( e.KeyData == Keys.Escape )
                    {
                        DataTable dt = (DataTable)dgv6.DataSource;
                        int rowHandle = gridMain6.FocusedRowHandle;

                        int row = gridMain6.FocusedRowHandle;
                        if (row >= 0)
                        {
                            DataRow dr = gridMain6.GetFocusedDataRow();

                            string str = dt.Rows[row]["data"].ObjToString();
                            dr["data"] = str;
                            CiLookup6_SelectedIndexChangedAgain(str);
                        }
                        gridMain6_ShownEditor(null, null);
                    }
                    //DataTable dt = (DataTable)dgv6.DataSource;
                    //int rowHandle = gridMain6.FocusedRowHandle;

                    //int row = gridMain6.FocusedRowHandle;
                    //if (row >= 0)
                    //{
                    //    string str = dt.Rows[row]["dbfield"].ObjToString();
                    //    GridColumn currCol = gridMain6.FocusedColumn;
                    //    string currentColumn = currCol.FieldName;
                    //    string data = "";
                    //}
                }
                catch ( Exception ex)
                {
                    MessageBox.Show("***ERROR*** Data Error.\n" + ex.Message.ToString(), "Data Entry Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                }
            }
        }
        /****************************************************************************************/
        private void gridMain6_CustomRowFilter(object sender, DevExpress.XtraGrid.Views.Base.RowFilterEventArgs e)
        {
            int row = e.ListSourceRow;
            GridView View = sender as GridView;
            DataTable dt = (DataTable)dgv6.DataSource;
            GridColumn currCol = gridMain6.FocusedColumn;
            string currentColumn = currCol.FieldName;

            string str = dt.Rows[row]["dbfield"].ObjToString();
            //string str = View.GetRowCellValue(row, "dbfield").ObjToString();
            if (str != null)
            {
                if (str.ToUpper() == "SRVCLERGY" || str.ToUpper() == "SRV2CLERGY")
                {
                    e.Visible = false;
                    e.Handled = true;
                }
            }
        }
        /****************************************************************************************/
        //private void repositoryItemComboBox21_SelectedIndexChanged(object sender, EventArgs e)
        //{
        //    DevExpress.XtraEditors.ComboBoxEdit menu = (DevExpress.XtraEditors.ComboBoxEdit)sender;
        //    if (menu == null)
        //        return;
        //    int row = menu.SelectedIndex;
        //    var item = menu.SelectedItem;
        //    if (!String.IsNullOrWhiteSpace(item.ObjToString()))
        //    {
        //        textBox1.Text = item.ObjToString();
        //        textBox1.Refresh();
        //    }
        //    else
        //    {
        //        DataRow dr = gridMain4.GetFocusedDataRow();
        //        string column = gridMain4.FocusedColumn.FieldName.Trim();
        //        textBox1.Text = dr[column].ObjToString();
        //    }
        //    string text = menu.SelectedText;
        //    //if (funDemo.Visible)
        //    //    funDemo.Hide();
        //}
        /****************************************************************************************/
        private void repositoryItemComboBox21_SelectedIndexChanged(object sender, EventArgs e)
        {
            DevExpress.XtraEditors.ComboBoxEdit menu = (DevExpress.XtraEditors.ComboBoxEdit)sender;
            if (menu == null)
                return;

            int row = menu.SelectedIndex;
            var item = menu.SelectedItem;
            if (!String.IsNullOrWhiteSpace(item.ObjToString()))
            {
                textBox1.Text = item.ObjToString();
                textBox1.Refresh();
            }
            else
            {
                DataRow dr = gridMain4.GetFocusedDataRow();
                string column = gridMain4.FocusedColumn.FieldName.Trim();
                textBox1.Text = dr[column].ObjToString();
            }
            string text = menu.SelectedText;
        }
        /****************************************************************************************/
        //private DevExpress.XtraEditors.Popup.ComboBoxPopupListBoxForm popupForm = null;
        private PopupListBoxForm popupForm = null;
        private PopupListBoxForm popupForm4 = null;
        private PopupListBoxForm popupForm6 = null;
        private string originalName = "";
        private void repositoryItemComboBox21_Popup(object sender, EventArgs e)
        {
            if (popupForm != null)
                return;

            DataTable dt = (DataTable)dgv4.DataSource;
            DataRow dr = gridMain4.GetFocusedDataRow();
            originalName = dr["depLastName"].ObjToString();

            //DevExpress.XtraEditors.ComboBoxEdit cbo = (DevExpress.XtraEditors.ComboBoxEdit)sender;
            //cbo.PreviewKeyDown += new PreviewKeyDownEventHandler(comboBox_PreviewKeyDown);

            //PopupListBoxForm popupForm = (sender as IPopupControl).PopupWindow as PopupListBoxForm;
            popupForm = (sender as IPopupControl).PopupWindow as PopupListBoxForm;
            //            popupForm.PreviewKeyDown += PopupForm_PreviewKeyDown; // Did not work
            popupForm.KeyUp += Cmb_KeyUp;
            popupForm.KeyPress += PopupForm_KeyPress;
            popupForm.ListBox.MouseMove += ListBox_MouseMove;
            popupForm.ListBox.MouseDown += ListBox_MouseDown;
            popupForm.ListBox.KeyUp += Cmb_KeyUp;
            popupForm.ListBox.KeyPress += PopupForm_KeyPress;
            popupForm.ListBox.SelectedValueChanged += ListBox_SelectedValueChanged;
        }
        /****************************************************************************************/
        private void repositoryItemComboBox22_Popup(object sender, EventArgs e)
        {
            if (popupForm4 != null)
                return;

            DataTable dt = (DataTable)dgv4.DataSource;
            DataRow dr = gridMain4.GetFocusedDataRow();
            originalName = dr["depFirstName"].ObjToString();

            //PopupListBoxForm popupForm = (sender as IPopupControl).PopupWindow as PopupListBoxForm;
            //popupForm = (sender as IPopupControl).PopupWindow as PopupListBoxForm;
            popupForm4 = (sender as IPopupControl).PopupWindow as PopupListBoxForm;
            popupForm4.ListBox.MouseMove += ListBox_MouseMove4;
            popupForm4.ListBox.MouseDown += ListBox_MouseDown;
            popupForm4.ListBox.KeyUp += Cmb_KeyUp;
            popupForm4.ListBox.SelectedValueChanged += ListBox_SelectedValueChanged4;
        }

        private void PopupForm_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
        }

        /****************************************************************************************/
        private void comboBox_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            //DevExpress.XtraEditors.ComboBoxEdit cbo = (DevExpress.XtraEditors.ComboBoxEdit)sender;
            //cbo.PreviewKeyDown -= comboBox_PreviewKeyDown;
            //if (!cbo.Focused) cbo.Focus();
        }
        /****************************************************************************************/
        private void PopupForm_KeyPress(object sender, KeyPressEventArgs e)
        {
        }
        /****************************************************************************************/
        private int lastIndex = -1;
        private FuneralDemo funDemo = null;
        private void ListBox_MouseMove(object sender, MouseEventArgs e)
        {
            PopupListBox listBoxControl = sender as PopupListBox;
            ComboBoxEdit cmb = listBoxControl.OwnerEdit as ComboBoxEdit;
            int index = listBoxControl.IndexFromPoint(new Point(e.X, e.Y));
            if (index < 0)
            {
            }
            else
            {
                if (index == lastIndex)
                    return;

                DataTable dt = (DataTable)dgv4.DataSource;
                DataRow dr = gridMain4.GetFocusedDataRow();
                string newName = dr["depLastName"].ObjToString();
                if (newName == originalName)
                {
                }

                whichRowChanged = gridMain4.FocusedRowHandle;

                string columnName = gridMain4.FocusedColumn.FieldName.ToUpper();
                if ( columnName == "DEPLASTNAME")
                {
                }


                string item = cmb.Properties.Items[index].ToString();
                //textBox1.Text = item; // ramma zamma 1
                //textBox1.Refresh();

                dr = gridMain4.GetFocusedDataRow();
                string xnewName = dr["depLastName"].ObjToString();
                if (xnewName != originalName)
                {
                }
                lastIndex = index;

                string title = "";
                string firstName = "";
                string middleName = "";
                string lastName = "";
                string suffix = "";
                string address = "";
                string city = "";
                string county = "";
                string state = "";
                string zip = "";
                string phone = "";
                string location = "";

                if (columnName == "DEPLASTNAME")
                {
                    string[] Lines = item.Split(',');
                    if (Lines.Length > 1)
                    {
                        firstName = Lines[1].Trim();
                        lastName = Lines[0].Trim();
                        if (lastName.ToUpper() == "WICKER")
                        {
                        }
                        //item = Lines[1].Trim() + " " + Lines[0].Trim();
                    }
                    else
                    {
                        lastName = item;
                    }
                }
                else if (columnName == "DEPFIRSTNAME") // ramma zamma 3
                {
                    string[] Lines = item.Split(' ');
                    if (Lines.Length > 1)
                    {
                        firstName = Lines[0].Trim();
                        lastName = Lines[1].Trim();
                        //item = Lines[1].Trim() + " " + Lines[0].Trim();
                    }
                    else
                    {
                        lastName = item;
                    }
                }

                try
                {
                    DataRow[] dRows = null;
                    if (columnName == "DEPLASTNAME")
                        dRows = clergyDt.Select("depLastName='" + lastName + "' AND depFirstName='" + firstName + "'");
                    else if (columnName == "DEPFIRSTNAME") // RAMMA ZAMMA 4
                        dRows = clergyDt.Select("depLastName='" + lastName + "' AND depFirstName='" + firstName + "'");
                    else
                        dRows = clergyDt.Select("answer='" + item + "'");
                    if (dRows.Length > 0)
                    {
                        //title = GetNonBlankData(dRows[0], dr, "depPrefix");
                        //firstName = GetNonBlankData(dRows[0], dr, "depFirstName");
                        //middleName = GetNonBlankData(dRows[0], dr, "depMiddleName");
                        //lastName = GetNonBlankData(dRows[0], dr, "depLastName");
                        //suffix = GetNonBlankData(dRows[0], dr, "depSuffix");
                        address = GetNonBlankData(dRows[0], dr, "address");
                        city = GetNonBlankData(dRows[0], dr, "city");
                        county = GetNonBlankData(dRows[0], dr, "county");
                        state = GetNonBlankData(dRows[0], dr, "state");
                        zip = GetNonBlankData(dRows[0], dr, "zip");
                        phone = GetNonBlankData(dRows[0], dr, "phone");


                        title = dRows[0]["depPrefix"].ObjToString();
                        firstName = dRows[0]["depFirstName"].ObjToString();
                        middleName = dRows[0]["depMI"].ObjToString();
                        lastName = dRows[0]["depLastName"].ObjToString();
                        suffix = dRows[0]["depSuffix"].ObjToString();
                        location = dRows[0]["location"].ObjToString();
                        address = dRows[0]["address"].ObjToString();
                        city = dRows[0]["city"].ObjToString();
                        county = dRows[0]["county"].ObjToString();
                        state = dRows[0]["state"].ObjToString();
                        zip = dRows[0]["zip"].ObjToString();
                        phone = dRows[0]["phone"].ObjToString();
                    }
                    else
                    {
                    }
                }
                catch (Exception ex)
                {
                }

                if (1 == 1)
                {
                    txtLastName.Text = lastName;
                    txtFirstName.Text = firstName;
                    txtTitle.Text = title;
                    txtMiddleName.Text = middleName;
                    txtSuffix.Text = suffix;
                    txtAddress2.Text = address;
                    txtCity2.Text = city;
                    txtState2.Text = state;
                    txtZip2.Text = zip;
                    txtPhone2.Text = phone;
                    if ( !panelClergyStuff.Visible )
                    {
                        panelClergyStuff.Show();
                        panelClergyStuff.Refresh();
                    }
                    return;
                }

                if (funDemo != null)
                {
                    try
                    {
                        funDemo.FireEventFunDemoLoad("Person", "Clergy", title, firstName, middleName, lastName, suffix, item, address, city, county, state, zip, phone, location);
                        funDemo.TopMost = true;
                        if (!funDemo.Visible)
                        {
                            funDemo.Visible = true;
                            funDemo.Refresh();
                        }
                    }
                    catch (Exception ex)
                    {
                        if (funDemo.IsDisposed)
                        {
                            funDemo = new FuneralDemo("Clergy", "", "", "", "", "", "", "", "", "", "", "");
                            funDemo.FunDemoDone += FunDemo_FunDemoDone;
                            funDemo.Show();
                            funDemo.Hide();
                        }
                    }
                }
                cmb.ShowPopup();
                //popupForm.Focus();
            }
        }
        /****************************************************************************************/
        private Rectangle rect4;
        private void ListBox_MouseMove4(object sender, MouseEventArgs e)
        {
            PopupListBox listBoxControl = sender as PopupListBox;
            ComboBoxEdit cmb = listBoxControl.OwnerEdit as ComboBoxEdit;
            int index = listBoxControl.IndexFromPoint(new Point(e.X, e.Y));
            if (index < 0)
            {
                btnClergyCancel_Click(null, null);
            }
            else
            {
                if (!panelClergyStuff.Visible)
                    panelClergyStuff.Show();

                if (index == lastIndex)
                    return;

                rect4 = cmb.Bounds;


                DataTable dt = (DataTable)dgv4.DataSource;
                DataRow dr = gridMain4.GetFocusedDataRow();
                string newName = dr["depLastName"].ObjToString();
                if (newName == originalName)
                {
                }

                whichRowChanged = gridMain4.FocusedRowHandle;

                string columnName = gridMain4.FocusedColumn.FieldName.ToUpper();
                if (columnName == "DEPLASTNAME")
                {
                }


                string item = cmb.Properties.Items[index].ToString();
                //textBox1.Text = item; // ramma zamma 1
                //textBox1.Refresh();

                dr = gridMain4.GetFocusedDataRow();
                string xnewName = dr["depLastName"].ObjToString();
                if (xnewName != originalName)
                {
                }
                lastIndex = index;

                string title = "";
                string firstName = "";
                string middleName = "";
                string lastName = "";
                string suffix = "";
                string address = "";
                string city = "";
                string county = "";
                string state = "";
                string zip = "";
                string phone = "";
                string location = "";

                if (columnName == "DEPLASTNAME")
                {
                    string[] Lines = item.Split(',');
                    if (Lines.Length > 1)
                    {
                        firstName = Lines[1].Trim();
                        lastName = Lines[0].Trim();
                        if (lastName.ToUpper() == "WICKER")
                        {
                        }
                        //item = Lines[1].Trim() + " " + Lines[0].Trim();
                    }
                    else
                    {
                        lastName = item;
                    }
                }
                else if (columnName == "DEPFIRSTNAME") // ramma zamma 3
                {
                    string[] Lines = item.Split(' ');
                    if (Lines.Length > 1)
                    {
                        firstName = Lines[0].Trim();
                        lastName = Lines[1].Trim();
                        //item = Lines[1].Trim() + " " + Lines[0].Trim();
                    }
                    else
                    {
                        lastName = item;
                    }
                }

                try
                {
                    DataRow[] dRows = null;
                    if (columnName == "DEPLASTNAME")
                        dRows = clergyDt.Select("depLastName='" + lastName + "' AND depFirstName='" + firstName + "'");
                    else if (columnName == "DEPFIRSTNAME") // RAMMA ZAMMA 4
                        dRows = clergyDt.Select("depLastName='" + lastName + "' AND depFirstName='" + firstName + "'");
                    else
                        dRows = clergyDt.Select("answer='" + item + "'");
                    if (dRows.Length > 0)
                    {
                        //title = GetNonBlankData(dRows[0], dr, "depPrefix");
                        //firstName = GetNonBlankData(dRows[0], dr, "depFirstName");
                        //middleName = GetNonBlankData(dRows[0], dr, "depMiddleName");
                        //lastName = GetNonBlankData(dRows[0], dr, "depLastName");
                        //suffix = GetNonBlankData(dRows[0], dr, "depSuffix");
                        address = GetNonBlankData(dRows[0], dr, "address");
                        city = GetNonBlankData(dRows[0], dr, "city");
                        county = GetNonBlankData(dRows[0], dr, "county");
                        state = GetNonBlankData(dRows[0], dr, "state");
                        zip = GetNonBlankData(dRows[0], dr, "zip");
                        phone = GetNonBlankData(dRows[0], dr, "phone");


                        title = dRows[0]["depPrefix"].ObjToString();
                        firstName = dRows[0]["depFirstName"].ObjToString();
                        middleName = dRows[0]["depMI"].ObjToString();
                        lastName = dRows[0]["depLastName"].ObjToString();
                        suffix = dRows[0]["depSuffix"].ObjToString();
                        location = dRows[0]["location"].ObjToString();
                        address = dRows[0]["address"].ObjToString();
                        city = dRows[0]["city"].ObjToString();
                        county = dRows[0]["county"].ObjToString();
                        state = dRows[0]["state"].ObjToString();
                        zip = dRows[0]["zip"].ObjToString();
                        phone = dRows[0]["phone"].ObjToString();
                    }
                }
                catch (Exception ex)
                {
                }
                if ( 1 == 1)
                {
                    txtLastName.Text = lastName;
                    txtFirstName.Text = firstName;
                    txtTitle.Text = title;
                    txtMiddleName.Text = middleName;
                    txtSuffix.Text = suffix;
                    txtAddress2.Text = address;
                    txtCity2.Text = city;
                    txtState2.Text = state;
                    txtZip2.Text = zip;
                    txtPhone2.Text = phone;
                    return;
                }

                if (funDemo != null)
                {
                    try
                    {
                        funDemo.FireEventFunDemoLoad("Person", "Clergy", title, firstName, middleName, lastName, suffix, item, address, city, county, state, zip, phone, location);
                        funDemo.TopMost = true;
                        if (!funDemo.Visible)
                        {
                            funDemo.Visible = true;
                            funDemo.Refresh();
                        }
                    }
                    catch (Exception ex)
                    {
                        if (funDemo.IsDisposed)
                        {
                            funDemo = new FuneralDemo("Clergy", "", "", "", "", "", "", "", "", "", "", "");
                            funDemo.FunDemoDone += FunDemo_FunDemoDone;
                            funDemo.Show();
                            funDemo.Hide();
                        }
                    }
                }
                cmb.ShowPopup();
                //popupForm.Focus();
            }
        }
        /****************************************************************************************/
        private void Cmb_KeyUp(object sender, KeyEventArgs e)
        {
            //if (e.KeyCode == Keys.Up)
            //    SendKeys.Send("{UP}");
            //else if (e.KeyCode == Keys.Down)
            //    SendKeys.Send("{DOWN}");

        }
        /****************************************************************************************/
        private string GetNonBlankData(DataRow dr, DataRow dr2, string field)
        {
            string answer = "";
            try
            {
                answer = dr2[field].ObjToString();
                if (String.IsNullOrWhiteSpace(answer))
                    answer = dr[field].ObjToString();
            }
            catch (Exception ex)
            {
            }
            return answer;
        }
        /****************************************************************************************/
        private void FunDemo_FunDemoDone(string title, string firstName, string middleName, string lastName, string suffix, string name, string address, string city, string county, string state, string zip, string phone)
        {
            if (title == "Cancel")
            {
                DataTable dx = (DataTable)dgv4.DataSource;
                this.textBox1.Text = "";
                this.textBox1.Refresh();
                return;
            }
            if (dgv4 == null)
                return;
            if (dgv4.DataSource == null)
                return;
            if (gridMain4 == null)
                return;

            try
            {
                DataRow dr = gridMain4.GetFocusedDataRow();
                if (dr == null)
                    return;
                if (whichRowChanged < 0)
                    return;

                int rowHandle = whichRowChanged;
                if (rowHandle >= 0)
                    dr = gridMain4.GetDataRow(rowHandle);
                if (dr == null)
                    return;

                int row = GetCurrentGridView().GetFocusedDataSourceRowIndex();

                DataTable dt = (DataTable)dgv4.DataSource;

                dr["fullName"] = name;
                dr["depPrefix"] = title;
                dr["depFirstName"] = firstName;
                dr["depMI"] = middleName;
                dr["depLastName"] = lastName;
                dr["depSuffix"] = suffix;
                dr["address"] = address;
                dr["city"] = city;
                dr["county"] = county;
                dr["state"] = state;
                dr["zip"] = zip;
                dr["phone"] = phone;
                dr["mod"] = "Y";

                funModified = true;
                btnSaveAll.Show();
                btnSaveAll.Refresh();

                gridMain4.RefreshData();
                gridMain4.RefreshEditor(true);
                dgv4.Refresh();
            }
            catch (Exception ex)
            {
            }
        }
        /****************************************************************************************/
        private void FunFamilyNew_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                if (funDemo != null)
                {
                    funDemo.Close();
                    funDemo = null;
                }
            }
            catch (Exception ex)
            {
            }
        }
        /****************************************************************************************/
        //private void ListBox_SelectedValueChanged(object sender, EventArgs e)
        //{
        //    //if (1 == 1) // Ramma Zamma 5
        //    //    return;
        //    DataRow dr = gridMain4.GetFocusedDataRow();
        //    string column = gridMain4.FocusedColumn.FieldName.Trim().ToUpper();
        //    if ( column == "DEPLASTNAME")
        //    {

        //    }
        //    //string item = dr["depFirstName"].ObjToString();
        //    string item = dr[column].ObjToString();
        //    if ( !String.IsNullOrWhiteSpace ( item ))
        //    {
        //    }
        //    item = popupForm.ListBox.SelectedValue.ObjToString();
        //    //dr["data"] = item;
        //    //gridMain6.RefreshData();
        //    //gridMain6.RefreshEditor(true);
        //    //textBox1.Text = item; ramma zamma 2
        //    //textBox1.Refresh();
        //    int index = popupForm.ListBox.SelectedIndex;
        //    if (index == lastIndex)
        //        return;
        //    if (!String.IsNullOrWhiteSpace(item))
        //    {
        //        if (!String.IsNullOrWhiteSpace(item))
        //        {
        //            textBox1.Text = item;
        //            textBox1.Refresh();
        //        }
        //    }
        //    //dgv4.Focus();
        //    //gridMain4.Focus();
        //}
        /****************************************************************************************/
        private void ListBox_SelectedValueChanged(object sender, EventArgs e)
        {
            //if (1 == 1) // Ramma Zamma 5
            //    return;

            DataRow dr = gridMain4.GetFocusedDataRow();
            string item = dr["depLastName"].ObjToString();
            if (!String.IsNullOrWhiteSpace(item))
            {
            }
            item = popupForm.ListBox.SelectedValue.ObjToString();
            int index = popupForm.ListBox.SelectedIndex;
            if (index == -1)
            {
                textBox1.Text = "";
                textBox1.Refresh();
            }
            if (index == lastIndex)
                return;
            if (index < 0)
            {
                textBox1.Text = "";
                textBox1.Refresh();
            }
            if (!String.IsNullOrWhiteSpace(item))
            {
                if (!String.IsNullOrWhiteSpace(item))
                {
                    textBox1.Text = item;
                    textBox1.Refresh();
                }
            }
            //gridMain4.Focus();
        }
        /****************************************************************************************/
        private void ListBox_MouseDown(object sender, MouseEventArgs e)
        {
            if ( panelClergyStuff.Visible )
            {
                LoadUpClergy();
            }
            if (funDemo != null)
            {
                if (!funDemo.IsDisposed)
                {
                    if (funDemo.Visible)
                    {
                        funDemo.Hide();
                        //popupForm.Close();
                        string item = textBox1.Text.Trim();
                        DataRow dr = gridMain4.GetFocusedDataRow();
                        dr = gridMain4.GetDataRow(whichRowChanged);
                        //dr["data"] = item;
                        //dr["mod"] = "Y";
                        gridMain4.RefreshData();
                        gridMain4.RefreshEditor(true);
                        this.Focus();
                        this.TopMost = true;
                        if (funDemo != null && !funDemo.IsDisposed)
                            funDemo.Close();
                    }
                }
            }
        }
        /****************************************************************************************/
        //private void repositoryItemComboBox21_KeyUp(object sender, KeyEventArgs e)
        //{
        //    //if (e.KeyCode == Keys.Up)
        //    //    SendKeys.Send("{UP}");
        //    //else if (e.KeyCode == Keys.Down)
        //    //    SendKeys.Send("{DOWN}");
        //    string column = gridMain4.FocusedColumn.FieldName.Trim().ToUpper();
        //    if (column != "DEPFIRSTNAME" && column != "DEPLASTNAME")
        //        return;
        //    if (e.KeyCode == Keys.Enter)
        //    {
        //        DataRow dr = gridMain4.GetFocusedDataRow();
        //        string name = dr[column].ObjToString();
        //        //FindClergyData(column, name, gridMain4.FocusedRowHandle);
        //    }
        //}
        /****************************************************************************************/
        private void repositoryItemComboBox21_KeyUp(object sender, KeyEventArgs e)
        {
            string column = gridMain4.FocusedColumn.FieldName.Trim().ToUpper();
            if (column != "DEPFIRSTNAME" && column != "DEPLASTNAME")
                return;

            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Tab )
            {
                if (SMFS.captureText)
                {
                    holdData = SMFS.capturedText;
                    holdData = G1.force_lower_line(holdData);
                    SMFS.captureText = false;
                    SMFS.capturedText = "";

                    if (holdData.ToUpper() != textBox1.Text.ToUpper())
                    {
                        if (!validateText21())
                        {
                            holdData = G1.force_lower_line(holdData);
                            textBox1.Text = holdData;
                            textBox1.Refresh();
                        }
                    }
                }
            }
        }
        /****************************************************************************************/
        private void dgv4_KeyUp(object sender, KeyEventArgs e)
        {
            //string column = gridMain4.FocusedColumn.FieldName.Trim().ToUpper();
            //if (column != "DEPFIRSTNAME" && column != "DEPLASTNAME")
            //    return;
            //if (e.KeyCode != Keys.Enter)
            //{
            //    holdData += e.KeyCode.ObjToString();
            //}
        }
        /****************************************************************************************/
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            string item = textBox1.Text;
            if (String.IsNullOrWhiteSpace(item))
            {
                gridMain6.Focus();
                return;
            }
            if (dgv6.Visible)
            {
                textBox1_TextChanged6(sender, e);
                return;
            }

            DataTable dt = (DataTable)dgv4.DataSource;
            string answer = item;
            string address = "";
            string city = "";
            string county = "";
            string state = "";
            string zip = "";
            string phone = "";
            string location = "";
            string middleName = "";
            string suffix = "";
            string title = "";
            bool breaking = false;
            bool found = false;
            //if (1 == 1)
            //    return;

            string firstName = "";
            string lastName = "";
            //if (1 == 1)
            //    return;

            string[] Lines = null;
            string column = gridMain4.FocusedColumn.FieldName;
            DataRow dr = gridMain4.GetFocusedDataRow();
            if (column.ToUpper() == "DEPLASTNAME")
            {
                Lines = item.Split(',');
                if (Lines.Length > 1)
                {
                    firstName = Lines[1].Trim();
                    lastName = Lines[0].Trim();
                    firstName = G1.force_lower_line(firstName);
                    lastName = G1.force_lower_line(lastName);
                }
                else
                {
                    firstName = dr["DEPFIRSTNAME"].ObjToString();
                    lastName = item;
                }
            }
            else if (column.ToUpper() == "DEPFIRSTNAME")
            {
                if ( item.EndsWith ("  "))
                {
                    breaking = true;
                    dr["depPrefix"] = "";
                    dr["DEPLASTNAME"] = "";
                    dr["depSuffix"] = "";
                    dr["address"] = "";
                    dr["city"] = "";
                    dr["state"] = "";
                    dr["zip"] = "";
                    dr["depMI"] = "";
                    dr["county"] = "";
                    dr["phone"] = "";
                }
                Lines = item.Split(' ');
                if (Lines.Length > 1)
                {
                    firstName = Lines[0].Trim();
                    lastName = Lines[1].Trim();
                    firstName = G1.force_lower_line(firstName);
                    lastName = G1.force_lower_line(lastName);
                }
                else
                {
                    firstName = item;
                    lastName = dr["DEPLASTNAME"].ObjToString();
                }
            }

            try
            {
                DataRow[] dRows = null;
                if (column.ToUpper() == "DEPLASTNAME")
                    dRows = clergyDt.Select("depLastName LIKE '%" + lastName + "%' AND depFirstName LIKE '%" + firstName + "%'");
                else if (column.ToUpper() == "DEPFIRSTNAME")
                    dRows = clergyDt.Select("depLastName LIKE '%" + lastName + "%' AND depFirstName LIKE '%" + firstName + "%'");
                else
                    dRows = clergyDt.Select("answer='" + item + "'");
                if (dRows.Length > 0)
                {
                    location = dRows[0]["location"].ObjToString();
                    address = dRows[0]["address"].ObjToString();
                    city = dRows[0]["city"].ObjToString();
                    county = dRows[0]["county"].ObjToString();
                    state = dRows[0]["state"].ObjToString();
                    zip = dRows[0]["zip"].ObjToString();
                    phone = dRows[0]["phone"].ObjToString();
                    suffix = dRows[0]["depSuffix"].ObjToString();
                    middleName = dRows[0]["depMI"].ObjToString();
                    title = dRows[0]["depPrefix"].ObjToString();
                    found = true;
                }
                else
                {
                }
            }
            catch (Exception ex)
            {
            }
            if (1 == 1)
            {
                if ( !found )
                {
                    title = dr["depPrefix"].ObjToString();
                    suffix = dr["depSuffix"].ObjToString();
                    address = dr["address"].ObjToString();
                    city = dr["city"].ObjToString();
                    state = dr["state"].ObjToString();
                    zip = dr["zip"].ObjToString();
                    middleName = dr["depMI"].ObjToString();
                    county = dr["county"].ObjToString();
                    phone = dr["phone"].ObjToString();
                }
                LoadClergyUp ( title, firstName, middleName, lastName, suffix, address, city, county, state, zip, phone, location );
                return;
            }

            try
            {
                if (funDemo == null || funDemo.IsDisposed)
                {
                    funDemo = new FuneralDemo("Person", "Clergy", item, "", "", "", "", "");
                    funDemo.FunDemoDone += FunDemo_FunDemoDone;
                    Rectangle rect = funDemo.Bounds;
                    int top = rect.Y;
                    int left = rect.X;
                    int height = rect.Height;
                    int width = rect.Width;
                    top = this.Bounds.Y;
                    left = this.Bounds.Width - width;
                    funDemo.StartPosition = FormStartPosition.Manual;
                    funDemo.SetBounds(left, top, width, height);

                    funDemo.Show();
                }
            }
            catch (Exception ex)
            {
            }

            if (funDemo != null && !funDemo.IsDisposed)
            {
                try
                {
                    funDemo.FireEventFunDemoLoad("Person", "Clergy", "", "", "", "", "", item, address, city, county, state, zip, phone, location, column );
                    funDemo.TopMost = true;
                    if (!funDemo.Visible && !funDemo.IsDisposed)
                    {
                        funDemo.Visible = true;
                        funDemo.Refresh();
                    }
                    //if (created)
                    if (popupForm6 == null)
                    {
                    }
                    if (popupForm6 != null)
                    {
                        popupForm6.ListBox.Show();
                        popupForm6.ListBox.Focus();
                        popupForm6.ListBox.Refresh();
                        popupForm6.ListBox.Visible = true;
                    }
                    //this.Focus();
                }
                catch (Exception ex)
                {
                    if (funDemo.IsDisposed)
                    {
                        funDemo = new FuneralDemo("Person", "", "", "", "", "", "", "", "", "", "", "");
                        funDemo.FunDemoDone += FunDemo_FunDemoDone;
                        funDemo.Show();
                        funDemo.Hide();
                    }
                }
            }

            try
            {
                //DataRow dr = gridMain4.GetFocusedDataRow();
                //string test = dr["depLastName"].ObjToString();
                //dr["depLastName"] = item;

                if ( !String.IsNullOrWhiteSpace ( column ))
                {
                    string sss = textBox1.Text;
                    //this.BringToFront();
                    //gridMain4.Focus(); // ramma zamma 6
                }

            }
            catch (Exception ex)
            {
            }
            //gridMain6.RefreshEditor(true);
            //popupForm.Focus();
            //cmb.ShowPopup();
        }
        /****************************************************************************************/
        private bool popupCreated = false;
        private void textBox1_TextChanged6(object sender, EventArgs e)
        {
            string item = textBox1.Text.Trim();
            if (String.IsNullOrWhiteSpace(item))
                return;
            string answer = item;
            string address = "";
            string city = "";
            string county = "";
            string state = "";
            string zip = "";
            string phone = "";
            string location = "";

            if (itemDt == null)
                return;
            try
            {
                DataRow[] dRows = null;
                dRows = itemDt.Select("answer='" + item + "'");
                if (dRows.Length > 0)
                {
                    location = dRows[0]["location"].ObjToString();
                    address = dRows[0]["address"].ObjToString();
                    city = dRows[0]["city"].ObjToString();
                    county = dRows[0]["county"].ObjToString();
                    state = dRows[0]["state"].ObjToString();
                    zip = dRows[0]["zip"].ObjToString();
                    phone = dRows[0]["phone"].ObjToString();
                }
            }
            catch (Exception ex)
            {
            }
            if (funDemo == null || funDemo.IsDisposed)
            {
                funDemo = new FuneralDemo("Place", editingWhat, item, "", "", "", "", "");
                funDemo.FunDemoDone += FunDemo_FunDemoDone6;
                Rectangle rect = funDemo.Bounds;
                int top = rect.Y;
                int left = rect.X;
                int height = rect.Height;
                int width = rect.Width;
                top = this.Bounds.Y;
                left = this.Bounds.Width - width;
                funDemo.StartPosition = FormStartPosition.Manual;
                funDemo.SetBounds(left, top, width, height);

                funDemo.Show();
                popupCreated = true;
            }

            if (funDemo != null && !funDemo.IsDisposed)
            {
                try
                {
                    funDemo.FireEventFunDemoLoad("Place", editingWhat, "", "", "", "", "", item, address, city, county, state, zip, phone, location);
                    funDemo.TopMost = true;
                    if (!funDemo.Visible && !funDemo.IsDisposed)
                    {
                        funDemo.Visible = true;
                        funDemo.Refresh();
                    }
                    //if (created)
                    popupForm6.ListBox.Show();
                    popupForm6.ListBox.Focus();
                    popupForm6.ListBox.Refresh();
                    popupForm6.ListBox.Visible = true;
                    //this.Focus();
                }
                catch (Exception ex)
                {
                    if (funDemo.IsDisposed)
                    {
                        funDemo = new FuneralDemo("Place", "", "", "", "", "", "", "", "", "", "", "");
                        funDemo.FunDemoDone += FunDemo_FunDemoDone6;
                        funDemo.Show();
                        funDemo.Hide();
                    }
                }
            }

            DataRow dr = gridMain6.GetFocusedDataRow();
            //dr["data"] = item; // ramma zamma
            //gridMain6.RefreshEditor(true);
            //popupForm.Focus();
            //cmb.ShowPopup();
        }
        /****************************************************************************************/
        private void BringFunDemoUp(string currentData)
        {
            //funDemo.FireEventFunDemoLoad("Place", editingWhat, "", "", "", "", "", currentData.Trim(), "", "", "", "", "", "", "");

            funDemo = new FuneralDemo("Place", currentData, "", "", "", "", "", "");
            this.Text = "Demographic Details for " + editingWhat;
            funDemo.FunDemoDone += FunDemo_FunDemoDone;
            Rectangle rect = funDemo.Bounds;
            int top = rect.Y;
            int left = rect.X;
            int height = rect.Height;
            int width = rect.Width;
            top = this.Bounds.Y;
            left = this.Bounds.Width - width;
            funDemo.StartPosition = FormStartPosition.Manual;
            funDemo.SetBounds(left, top, width, height);

            funDemo.Show();
            popupCreated = true;
        }
        /****************************************************************************************/
        private void LoadClergyUp (string title, string firstName, string middleName, string lastName, string suffix, string address, string city, string county, string state, string zip, string phone, string location)
        {
            txtTitle.Text = title;
            txtFirstName.Text = firstName;
            txtMiddleName.Text = middleName;
            txtLastName.Text = lastName;
            txtSuffix.Text = suffix;
            txtAddress2.Text = address;
            txtCity2.Text = city;
            txtCounty2.Text = county;
            txtState2.Text = state;
            txtZip2.Text = zip;
            txtPhone2.Text = phone;
            //txtLocation.Text = location;
            //txtLocationPlace.Text = location;
        }
        /****************************************************************************************/
        private string holdData = "";
        private string holdKey = "";
        //private void repositoryItemComboBox21_Leave(object sender, EventArgs e)
        //{
        //    DataRow dr = gridMain4.GetFocusedDataRow();
        //    string column = gridMain4.FocusedColumn.FieldName.Trim();

        //    holdData = dr[column].ObjToString();

        //    holdData = SMFS.capturedText;
        //    SMFS.captureText = false;

        //    LoadUpClergy();
        //}
        /****************************************************************************************/
        private void repositoryItemComboBox21_Leave(object sender, EventArgs e)
        {
            DataRow dr = gridMain4.GetFocusedDataRow();
            string column = gridMain4.FocusedColumn.FieldName.Trim();

            string cellData = dr[column].ObjToString();
            try
            {
                if (cellData.IndexOf(",") > 0 && !panelClergyStuff.Visible)
                {
                    panelClergyStuff.Show();
                    panelClergyStuff.Refresh();
                    SMFS.capturedText = cellData;
                }

                if (SMFS.captureText)
                {
                    holdData = SMFS.capturedText;
                    holdData = G1.force_lower_line(holdData);
                    SMFS.captureText = false;
                    SMFS.capturedText = "";

                    if (lastIndex < 0 && !String.IsNullOrWhiteSpace(holdData))
                    {
                        if (!validateText21())
                        {
                            holdData = G1.force_lower_line(holdData);
                            textBox1.Text = holdData;
                            textBox1.Refresh();

                            btnClergyAccept.Show();
                            btnClergyAccept.Hide();

                            btnClergyCancel.Show();
                            btnClergyCancel.Refresh();
                        }
                    }
                }
            }
            catch ( Exception ex)
            {
            }

            LoadUpClergy();
            gameOver = false;
        }
        /****************************************************************************************/
        private void LoadUpClergy()
        {
            if (!panelClergyStuff.Visible)
                return;
            string address = "";
            string city = "";
            string county = "";
            string state = "";
            string zip = "";
            string phone = "";

            string title = txtTitle.Text.Trim();
            string firstName = txtFirstName.Text.Trim();
            string middleName = txtMiddleName.Text.Trim();
            string lastName = txtLastName.Text.Trim();
            string suffix = txtSuffix.Text.Trim();
            //string name = txtName.Text.Trim();
            address = txtAddress2.Text.Trim();
            city = txtCity2.Text.Trim();
            county = txtCounty2.Text.Trim();
            state = txtState2.Text.Trim();
            zip = txtZip2.Text.Trim();
            phone = txtPhone2.Text.Trim();

            string name = G1.BuildFullName(title, firstName, middleName, lastName, suffix);


            DataRow dr = gridMain4.GetFocusedDataRow();

            dr["fullName"] = name;
            dr["depPrefix"] = title;
            dr["depFirstName"] = firstName;
            dr["depMI"] = middleName;
            dr["depLastName"] = lastName;
            dr["depSuffix"] = suffix;
            dr["address"] = address;
            dr["city"] = city;
            dr["county"] = county;
            dr["state"] = state;
            dr["zip"] = zip;
            dr["phone"] = phone;
            dr["mod"] = "Y";

            funModified = true;
            btnSaveAll.Show();
            btnSaveAll.Refresh();

            panelClergyStuff.Hide();
            dgv4.Dock = DockStyle.Fill;

            gridMain4.RefreshData();
            gridMain4.RefreshEditor(true);
            dgv4.Refresh();
        }
        /****************************************************************************************/
        private void repositoryItemComboBox21_Enter(object sender, EventArgs e)
        {
            Rectangle rect = dgv4.Bounds;
            int left = rect.Width / 2;
            int right = dgv4.Right;
            int top = rect.Height / 2;
            int bottom = panelClergyStuff.Bottom;

            int width = panelClergyStuff.Width;
            int height = panelClergyStuff.Height;

            panelClergyStuff.SetBounds(left, top, width, height);

            //panelClergyStuff.Show();

            //btnHold.Show();
            //btnHold.Refresh();

            btnClergyAccept.Hide();
            btnClergyAccept.Refresh();

            btnClergyCancel.Hide();
            btnClergyCancel.Refresh();

            SMFS.capturedText = "";
            SMFS.captureText = true;

            textBox1.Text = "";
            textBox1.Refresh();
        }
        /****************************************************************************************/
        private void btnHold_Click(object sender, EventArgs e)
        {
            string column = gridMain4.FocusedColumn.FieldName.Trim();
            DataRow dr = gridMain4.GetFocusedDataRow();

            string[] Lines = null;

            string firstName = "";
            string lastName = "";
            string name = dr[column].ObjToString();
            if (!String.IsNullOrWhiteSpace(holdData))
                name = holdData;
            holdData = "";
            if (column.ToUpper() == "DEPFIRSTNAME")
            {
                Lines = name.Split(' ');
                firstName = Lines[0];
                if ( Lines.Length > 1 )
                    lastName = Lines[1];
                else
                {
                    lastName = dr["depLastName"].ObjToString();
                }
            }
            else if (column.ToUpper() == "DEPLASTNAME")
            {
                Lines = name.Split(',');
                if (Lines.Length > 1)
                    firstName = Lines[1];
                else
                    firstName = dr["depFirstName"].ObjToString();
                lastName = Lines[0];
            }
            else
            {
                firstName = dr["depFirstName"].ObjToString();
                lastName = dr["depLastName"].ObjToString();
            }

            txtFirstName.Text = firstName;
            txtLastName.Text = lastName;

            Rectangle rect = dgv4.Bounds;
            int left = rect.Width / 2;
            int right = dgv4.Right;
            int top = rect.Height / 2;
            int bottom = panelClergyStuff.Bottom;

            int width = panelClergyStuff.Width;
            int height = panelClergyStuff.Height;

            panelClergyStuff.SetBounds(left, top, width, height);

            //dgv4.Dock = DockStyle.Left;
            panelClergyStuff.Show();


            btnClergyAccept.Show();
            btnClergyAccept.Refresh();

            btnClergyCancel.Show();
            btnClergyCancel.Refresh();
        }
        /****************************************************************************************/
        private void btnClergyCancel_Click(object sender, EventArgs e)
        {
            panelClergyStuff.Hide();
            dgv4.Dock = DockStyle.Fill;

            gridMain4.RefreshEditor(true);
            dgv4.Refresh();
        }
        /****************************************************************************************/
        private void btnClergyAccept_Click(object sender, EventArgs e)
        {
            LoadUpClergy();
        }
        /****************************************************************************************/
        private void repositoryItemComboBox22_SelectedIndexChanged(object sender, EventArgs e)
        {
            DevExpress.XtraEditors.ComboBoxEdit menu = (DevExpress.XtraEditors.ComboBoxEdit)sender;
            if (menu == null)
                return;

            int row = menu.SelectedIndex;
            var item = menu.SelectedItem;
            if (!String.IsNullOrWhiteSpace(item.ObjToString()))
            {
                textBox1.Text = item.ObjToString();
                textBox1.Refresh();
            }
            else
            {
                DataRow dr = gridMain4.GetFocusedDataRow();
                string column = gridMain4.FocusedColumn.FieldName.Trim();
                textBox1.Text = dr[column].ObjToString();
            }
            string text = menu.SelectedText;
        }
        /****************************************************************************************/
        private void repositoryItemComboBox22_Leave(object sender, EventArgs e)
        {
            DataRow dr = gridMain4.GetFocusedDataRow();
            if (dr == null)
                return;
            string column = gridMain4.FocusedColumn.FieldName.Trim();

            try
            {
                string cellData = dr[column].ObjToString();
                if (cellData.IndexOf(" ") > 0 && !panelClergyStuff.Visible)
                {
                    panelClergyStuff.Show();
                    panelClergyStuff.Refresh();
                }

                if (!panelClergyStuff.Visible)
                    return;

                if (SMFS.captureText)
                {
                    holdData = SMFS.capturedText;
                    holdData = G1.force_lower_line(holdData);
                    SMFS.captureText = false;
                    SMFS.capturedText = "";

                    if (lastIndex < 0 && !String.IsNullOrWhiteSpace(holdData))
                    {
                        textBox1.Text = holdData;
                        textBox1.Refresh();
                        btnClergyAccept.Show();
                        btnClergyAccept.Hide();

                        btnClergyCancel.Show();
                        btnClergyCancel.Refresh();

                        dr["add"] = "+";
                        dr["edit"] = "E";
                    }
                }

                if (!gameOver)
                    LoadUpClergy();
            }
            catch ( Exception ex)
            {
            }
        }
        /****************************************************************************************/
        private bool validateText21()
        {
            bool found = false;
            if (1 == 1)
                return false;
            string data = textBox1.Text.Trim().ToUpper();
            string str = "";
            for (int i = 0; i < repositoryItemComboBox21.Items.Count; i++)
            {
                str = repositoryItemComboBox21.Items[i].ObjToString().Trim().ToUpper();
                if (str == data)
                {
                    found = true;
                    break;
                }
            }
            return found;
        }
        /****************************************************************************************/
        private bool validateText22 ()
        {
            bool found = false;
            if (gameOver)
                return false;
            //if (1 == 1)
            //    return false;
            string data = textBox1.Text.Trim().ToUpper();
            string str = "";
            for ( int i=0; i< repositoryItemComboBox22.Items.Count; i++)
            {
                str = repositoryItemComboBox22.Items[i].ObjToString().Trim().ToUpper();
                if ( str == data )
                {
                    found = true;
                    break;
                }
            }
            return found;
        }
        /****************************************************************************************/
        private void repositoryItemComboBox22_KeyUp(object sender, KeyEventArgs e)
        {
            string column = gridMain4.FocusedColumn.FieldName.Trim().ToUpper();
            if (column != "DEPFIRSTNAME" && column != "DEPLASTNAME")
                return;

            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Tab )
            {
                if (SMFS.captureText)
                {
                    holdData = SMFS.capturedText;
                    holdData = G1.force_lower_line(holdData);
                    SMFS.captureText = false;
                    SMFS.capturedText = "";

                    if ( holdData.ToUpper() != textBox1.Text.ToUpper())
                    {
                        if (!validateText22() )
                        {
                            holdData = G1.force_lower_line(holdData);
                            textBox1.Text = holdData;
                            textBox1.Refresh();

                            DataRow dr = gridMain4.GetFocusedDataRow();
                            dr["add"] = "+";
                            dr["edit"] = "E";
                        }
                        else
                        {
                            if (!panelClergyStuff.Visible)
                                panelClergyStuff.Show();
                            textBox1_TextChanged(null, null);
                            LoadUpClergy();
                        }
                    }
                }
                //gameOver = false;
            }
            else
            {
                if (!SMFS.captureText)
                    return;
                gameOver = false;
                holdData = SMFS.capturedText;
                holdData = G1.force_lower_line(holdData);
                if ( holdData.EndsWith ( "  "))
                {
                    gameOver = true;
                }
            }
        }
        /****************************************************************************************/
        private void repositoryItemComboBox22_Enter(object sender, EventArgs e)
        {
            Rectangle rect = dgv4.Bounds;
            int left = rect.Width / 2;
            int right = dgv4.Right;
            int top = rect.Height / 2;
            int bottom = panelClergyStuff.Bottom;

            int width = panelClergyStuff.Width;
            int height = panelClergyStuff.Height;

            panelClergyStuff.SetBounds(left, top, width, height);

            //dgv4.Dock = DockStyle.Left;
            //panelClergyStuff.Show();

            //btnHold.Show();
            //btnHold.Refresh();

            btnClergyAccept.Hide();
            btnClergyAccept.Refresh();

            btnClergyCancel.Hide();
            btnClergyCancel.Refresh();

            SMFS.capturedText = "";
            SMFS.captureText = true;

            textBox1.Text = "";
            textBox1.Refresh();
        }
        /****************************************************************************************/
        private void ListBox_SelectedValueChanged4(object sender, EventArgs e)
        {
            //if (1 == 1) // Ramma Zamma 5
            //    return;

            DataRow dr = gridMain4.GetFocusedDataRow();
            string item = dr["depFirstName"].ObjToString();
            if (!String.IsNullOrWhiteSpace(item))
            {
            }
            item = popupForm4.ListBox.SelectedValue.ObjToString();
            int index = popupForm4.ListBox.SelectedIndex;
            if ( index == -1 )
            {
                textBox1.Text = "";
                textBox1.Refresh();
            }
            if (index == lastIndex)
                return;
            if (!String.IsNullOrWhiteSpace(item))
            {
                if (!String.IsNullOrWhiteSpace(item))
                {
                    textBox1.Text = item;
                    textBox1.Refresh();
                }
            }
            //gridMain4.Focus();
        }
        /****************************************************************************************/
        private void gridMain4_MouseMove(object sender, MouseEventArgs e)
        {
            if (panelClergyStuff.Visible)
                panelClergyStuff.Hide();
        }
        /****************************************************************************************/
        private void gridMain6_MouseMove(object sender, MouseEventArgs e)
        {
            if (funDemo == null)
                return;
            if (funDemo.Visible)
                funDemo.Hide();
        }
        /****************************************************************************************/
        private void gridMainDep_ShownEditor(object sender, EventArgs e)
        {
            GridColumn currCol = gridMainDep.FocusedColumn;
            currentColumn = currCol.FieldName;
            int focusedRow = gridMainDep.FocusedRowHandle;
            int row = gridMainDep.GetDataSourceRowIndex(focusedRow);
            DataRow dr = gridMainDep.GetFocusedDataRow();
            if (currentColumn.ToUpper() == "PHONE")
            {
                string phone = dr["phone"].ObjToString();
                return;
            }
        }
        /****************************************************************************************/
        private void gridMainDep_ValidatingEditor(object sender, BaseContainerValidateEditorEventArgs e)
        {
            GridView view = sender as GridView;
            if (view.FocusedColumn.FieldName.ToUpper() == "PHONE")
            {
                //DataTable dt = (DataTable)dgv.DataSource;
                DataRow dr = gridMainDep.GetFocusedDataRow();
                int rowhandle = gridMainDep.FocusedRowHandle;
                int row = gridMainDep.GetDataSourceRowIndex(rowhandle);
                string phone = e.Value.ObjToString();
                string newPhone = AgentProspectReport.reformatPhone(phone, true );
                e.Value = newPhone;
            }
        }
        /****************************************************************************************/
        void FunFamilyNew_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 39 )
            {
                e.KeyChar = '`';
                e.Handled = false;
            }
        }
        /****************************************************************************************/
    }
}