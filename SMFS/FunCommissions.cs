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
using DevExpress.Utils;
using System.IO;
/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public partial class FunCommissions : DevExpress.XtraEditors.XtraForm
    {
        private bool modified = false;
        private bool loading = true;
        private DataTable workDt = null;
        private string workManager = "";
        private string workArranger = "";
        private string workWho = "";
        private bool workBatch = false;
        private bool finale = false;
        private bool workPDF = false;
        private double workCommission = 0D;
        /***********************************************************************************************/
        public FunCommissions( DataTable dt, string manager, string arranger, string who, bool batchRun )
        {
            InitializeComponent();
            workDt = dt;
            workManager = manager;
            workArranger = arranger;
            workWho = who;
            workBatch = batchRun;
            finale = false;
        }
        /***********************************************************************************************/
        public FunCommissions( DataTable finalDt, string who, double commission )
        {
            InitializeComponent();
            workDt = finalDt;
            workManager = "";
            workArranger = "";
            workWho = who;
            workBatch = false;
            workCommission = commission;
            finale = true;
        }
        /***********************************************************************************************/
        private void FunCommissions_Load(object sender, EventArgs e)
        {
            picRowDown.Hide();
            picRowUp.Hide();
            btnSave.Hide();

            string title = "Funeral Commission for ";
            if ( workWho.ToUpper() == "MA" )
                title += " " + workManager + " as Both Manager and Arranger";
            else if ( workWho.ToUpper() == "M")
                title += " " + workManager + " as Manager";
            else if (workWho.ToUpper() == "A")
                title += " " + workArranger + " as Arranger";

            if (finale)
            {
                title = "Funeral Commission for ";
                if (workWho.ToUpper() == "M")
                    title += "All Managers";
                else if (workWho.ToUpper() == "A")
                    title += "All Arrangers";

                this.Text = title;

                SetupTotalsSummary();

                gridMain.Columns["name"].GroupIndex = 0;
                gridMain.Columns["name"].Visible = false;
                workDt.Rows.Clear();

                DataRow dR = workDt.NewRow();
                dR["option"] = "Total Commissions";
                dR["commission"] = workCommission;
                workDt.Rows.Add(dR);

                dgv.DataSource = workDt;

                gridMain.ExpandAllGroups();

                workBatch = true;

                printPreviewToolStripMenuItem_Click(null, null);

                this.Close();
                return;

                modified = false;
                loading = false;
            }

            else
            {
                this.Text = title;
                SetupTotalsSummary();
                gridMain.Columns["name"].Visible = false;
                LoadData();

                if (workBatch)
                    return;


                //gridMain.Columns["name"].GroupIndex = 0;

                //dgv.DataSource = workDt;

                //SetupTotalsSummary();

                //gridMain.ExpandAllGroups();

                modified = false;
                loading = false;

                if (!workBatch)
                {
                    btnInsert.Hide();
                    picRowDown.Hide();
                    picRowUp.Hide();

                    SetupTotalsSummary();
                }
            }
        }
        /****************************************************************************************/
        private void SetupTotalsSummary()
        {
            AddSummaryColumn("commission", null);
        }
        /****************************************************************************************/
        private void AddSummaryColumn(string columnName, DevExpress.XtraGrid.Views.Grid.GridView gMain = null)
        {
            if (gMain == null)
                gMain = gridMain;
            gMain.Columns[columnName].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Sum;
            gMain.Columns[columnName].SummaryItem.DisplayFormat = "{0:N2}";
        }
        /***********************************************************************************************/
        private void LoadData()
        {
            string cmd = "Select * from `funcommissiondata` where `name` = '" + workManager + "' OR `name` = '" + workArranger + "';";
            DataTable dt = G1.get_db_data(cmd);

            if ( dt.Rows.Count <= 0 )
            {
                string who = "";
                string option = "";
                string data = "";
                DataRow dRow = null;

                DataTable funDt = G1.get_db_data("Select * from `funcommoptions` ORDER by `order`;");
                for ( int i=0; i<funDt.Rows.Count; i++)
                {
                    who = funDt.Rows[i]["who"].ObjToString();
                    option = funDt.Rows[i]["option"].ObjToString();
                    data = funDt.Rows[i]["defaults"].ObjToString();

                    dRow = dt.NewRow();
                    dRow["name"] = workManager;
                    dRow["ma"] = who;
                    dRow["option"] = option;
                    dRow["answer"] = data;
                    dt.Rows.Add(dRow);
                }
            }

            dt.Columns.Add("num");
            dt.Columns.Add("mod");
            G1.NumberDataTable(dt);

            string what = "";

            for ( int i=(dt.Rows.Count-1); i>=0; i--)
            {
                what = dt.Rows[i]["ma"].ObjToString();
                if (what.ToUpper() != workWho.ToUpper())
                    dt.Rows.RemoveAt(i);
            }

            LoadFuneralDetails(dt);

            dgv.DataSource = dt;
            modified = false;
            loading = false;

            if ( workBatch )
            {
                printPreviewToolStripMenuItem_Click(null, null);
                double commission = 0D;
                for (int i = 0; i < dt.Rows.Count; i++)
                    commission += dt.Rows[i]["commission"].ObjToDouble();
                OnClosing(dt, commission );
                this.Close();
                return;
            }

        }
        /***********************************************************************************************/
        private void LoadFuneralDetails(DataTable dt)
        {
            string option = "";
            string answer = "";
            string ma = "";
            double count = 0D;
            double detail = 0D;
            bool processOption = false;

            if (G1.get_column_number(dt, "count") < 0)
                dt.Columns.Add("count", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "detail") < 0)
                dt.Columns.Add("detail", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "commission") < 0)
                dt.Columns.Add("commission", Type.GetType("System.Double"));

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                option = dt.Rows[i]["option"].ObjToString();
                answer = dt.Rows[i]["answer"].ObjToString();
                ma = dt.Rows[i]["ma"].ObjToString().ToUpper();

                processOption = false;
                if (workWho.ToUpper() == "MA")
                    processOption = true;
                else if (workWho.ToUpper() == "M" && ma == "M")
                    processOption = true;
                else if (workWho.ToUpper() == "A" && ma == "A")
                    processOption = true;

                if (!processOption)
                    continue;

                ParseOutOption(workDt, option, answer, ma, ref count, ref detail);

                dt.Rows[i]["count"] = count;
                dt.Rows[i]["detail"] = detail;
                if (detail > 0D)
                {
                    if ( option != "Funeral Average")
                        dt.Rows[i]["commission"] = detail;
                }
            }

            count = 0D;
            for (int i = 0; i < workDt.Rows.Count; i++)
            {
                answer = workDt.Rows[i]["funeralType"].ObjToString();
                if (answer.IndexOf("M") >= 0)
                {
                    answer = workDt.Rows[i]["gotPackage"].ObjToString();
                    if (String.IsNullOrWhiteSpace(answer))
                    {
                        answer = workDt.Rows[i]["urn"].ObjToString();
                        detail = answer.ObjToDouble();
                        if ( detail == 0D)
                            count++;
                    }
                }
            }

            double dollarsPerFuneral = 0D;
            double minimumFunerals = 0D;
            double funeralAverage = 0D;
            double averageMinimum = 0D;
            DataRow[] dRows = dt.Select("option='Funeral Average'");
            if (dRows.Length > 0)
            {
                funeralAverage = dRows[0]["detail"].ObjToDouble();
                averageMinimum = dRows[0]["answer"].ObjToDouble();
            }

            if (funeralAverage > averageMinimum)
            {
                dRows = dt.Select("option='Minimum Funerals'");
                if (dRows.Length > 0)
                {
                    minimumFunerals = dRows[0]["answer"].ObjToDouble();
                    dRows[0]["count"] = count;
                    dRows[0]["detail"] = count - minimumFunerals;
                }

                dRows = dt.Select("option='Dollars per Funeral'");
                if (dRows.Length > 0)
                    dollarsPerFuneral = dRows[0]["answer"].ObjToDouble();


                dRows = dt.Select("option='Dollars per Funeral'");
                if (dRows.Length > 0)
                {
                    dRows[0]["count"] = (count - minimumFunerals);
                    if (count > minimumFunerals)
                    {
                        dRows[0]["detail"] = (count - minimumFunerals) * dollarsPerFuneral;
                        dRows[0]["commission"] = (count - minimumFunerals) * dollarsPerFuneral;
                    }
                }
            }
        }
        /***********************************************************************************************/
        private void ParseOutOption ( DataTable dt, string option, string answer, string ma, ref double count, ref double detail )
        {
            count = 0D;
            detail = 0D;
            string who = "";
            string str = "";
            double dValue = 0D;
            double gauge = 0D;
            string[] Lines = null;
            if (option == "Funeral Average")
            {
                double netFuneral = 0D;
                double totalNet = 0D;
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    who = dt.Rows[i]["funeralType"].ObjToString();
                    if (who == ma || who == "MA" && ma == "M")
                    {
                        str = dt.Rows[i]["funeralType"].ObjToString();
                        if (str.IndexOf("M") >= 0)
                        {
                            str = dt.Rows[i]["gotPackage"].ObjToString();
                            if (String.IsNullOrWhiteSpace(str))
                            {
                                str = workDt.Rows[i]["urn"].ObjToString();
                                dValue = str.ObjToDouble();
                                if (dValue == 0D)
                                {
                                    count++;
                                    netFuneral = dt.Rows[i]["netFuneral"].ObjToDouble();
                                    totalNet += netFuneral;
                                }
                            }
                        }
                    }
                }
                if (count > 0D)
                {
                    detail = totalNet / count;
                    detail = G1.RoundValue(detail);
                }
            }
            else if (option == "Vault")
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    who = dt.Rows[i]["funeralType"].ObjToString();
                    if (who == ma || who == "MA" && ma == "M")
                    {
                        str = dt.Rows[i]["vault"].ObjToString();
                        if (!String.IsNullOrWhiteSpace(str))
                            count++;
                    }
                    detail = count * answer.ObjToDouble();
                }
            }
            else if (option == "Urn")
            {
                double totalUrn = 0D;
                answer = answer.Replace("%", "");
                double percent = 0D;
                if ( G1.validate_numeric ( answer))
                    percent = answer.ObjToDouble() / 100D;
                if (percent > 0D)
                {
                    count = 0;
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        who = dt.Rows[i]["funeralType"].ObjToString();
                        if (who == ma || who == "MA" && ma == "M")
                        {
                            str = dt.Rows[i]["urn"].ObjToString();
                            if (!String.IsNullOrWhiteSpace(str))
                            {
                                dValue = str.ObjToDouble();
                                if (dValue > 0D)
                                {
                                    dValue = dValue * percent;
                                    totalUrn += dValue;
                                    count++;
                                }
                            }
                        }
                        if (count > 0D)
                            detail = totalUrn;
                    }
                }
            }
            else if (option.ToUpper().IndexOf ( "CASKET GAUGE" ) == 0 )
            {
                option = ParseOutEqual(option);
                option = option.ToUpper().Replace("GAUGE", "").Trim();
                gauge = option.ObjToDouble();
                count = 0;
                if (gauge > 0D)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        who = dt.Rows[i]["funeralType"].ObjToString();
                        if (who == ma || who == "MA" && ma == "A")
                        {
                            str = dt.Rows[i]["casketgauge"].ObjToString();
                            if (!String.IsNullOrWhiteSpace(str))
                            {
                                dValue = str.ObjToDouble();
                                if (dValue == gauge)
                                    count++;
                            }
                        }
                    }
                }
                detail = count * answer.ObjToDouble();
            }
            else if (option.ToUpper().IndexOf ("CASKET TYPE") == 0)
            {
                count = 0;
                option = ParseOutEqual(option);
                if (!String.IsNullOrWhiteSpace(option))
                {
                    if (option.IndexOf("+") < 0)
                    {
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            who = dt.Rows[i]["funeralType"].ObjToString();
                            if (who == ma || who == "MA" && ma == "A")
                            {
                                str = dt.Rows[i]["caskettype"].ObjToString();
                                if (str.ToUpper().Trim() == option.ToUpper().Trim())
                                    count++;
                            }
                        }
                    }
                    else
                    {
                        Lines = option.Split('+');
                        if ( Lines.Length > 1 )
                        {
                            option = Lines[0].ToUpper();
                            Lines = Lines[1].Trim().Split(',');
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {
                                who = dt.Rows[i]["funeralType"].ObjToString();
                                if (who == ma || who == "MA" && ma == "A")
                                {
                                    str = dt.Rows[i]["caskettype"].ObjToString();
                                    if (str.ToUpper().Trim() == option.ToUpper().Trim())
                                    {
                                        str = dt.Rows[i]["casketdesc"].ObjToString().Trim().ToUpper();
                                        for (int j = 0; j < Lines.Length; j++)
                                        {
                                            if (!String.IsNullOrWhiteSpace(Lines[j]))
                                            {
                                                if (str.IndexOf(Lines[j].Trim().ToUpper()) >= 0)
                                                    count++;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                detail = count * answer.ObjToDouble();
            }
        }
        /***********************************************************************************************/
        private string ParseOutEqual ( string option )
        {
            string[] Lines = option.Split('=');
            if (Lines.Length <= 1)
                return option;
            option = Lines[1].Trim();
            return option;
        }
        /***********************************************************************************************/
        public static string GetCommOptionAnswer(string option)
        {
            string rv = "";
            DataTable dt = G1.get_db_data("Select * from `funcommoptions` where `option` = '" + option + "';");
            if (dt.Rows.Count <= 0)
                return rv;
            rv = dt.Rows[0]["answer"].ObjToString();
            return rv;
        }
        /***********************************************************************************************/
        private void gridMain_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string record = dr["record"].ObjToString();
            dr["mod"] = "Y";
            modified = true;
            btnSave.Show();
        }
        /***********************************************************************************************/
        private void AdminOptions_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!modified)
                return;
            DialogResult result = MessageBox.Show("***Question***\nCommission Options have been modified!\nWould you like to save your changes?", "Add/Edit Funeral Commission Options Dialog", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (result == DialogResult.Cancel)
            {
                e.Cancel = true;
                return;
            }
            modified = false;
            if (result == DialogResult.No)
                return;
            btnSave_Click(null, null);
        }
        /***********************************************************************************************/
        private void btnSave_Click(object sender, EventArgs e)
        {
            //string record = "";
            //string option = "";
            //string who = "";
            //string defaults = "";
            //string mod = "";
            //DataTable dt = (DataTable)dgv.DataSource;
            //for ( int i=0; i<dt.Rows.Count; i++)
            //{
            //    mod = dt.Rows[i]["mod"].ObjToString();
            //    //if (mod != "Y")
            //    //    continue;
            //    dt.Rows[i]["mod"] = "";
            //    record = dt.Rows[i]["record"].ObjToString();
            //    if (record == "-1" || String.IsNullOrWhiteSpace ( record ) || record == "0" )
            //        record = G1.create_record("funcommoptions", "defaults", "-1");
            //    if (G1.BadRecord("funcommoptions", record))
            //        continue;
            //    who = dt.Rows[i]["who"].ObjToString();
            //    option = dt.Rows[i]["option"].ObjToString();
            //    defaults = dt.Rows[i]["defaults"].ObjToString();
            //    G1.update_db_table("funcommoptions", "record", record, new string[] { "option", option, "defaults", defaults, "who", who, "order", i.ToString() });
            //    dt.Rows[i]["record"] = record.ObjToInt32();
            //}
            //modified = false;
            //btnSave.Hide();
            ////this.Close();
        }
        /***********************************************************************************************/
        private void pictureBox3_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            DataRow dRow = dt.NewRow();
            dRow["option"] = "New Option";
            dRow["mod"] = "Y";
            dRow["record"] = -1;
            dt.Rows.Add(dRow);
            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
            dgv.Refresh();
            gridMain.MoveLast();
            btnSave.Visible = true;
            btnSave.Refresh();
            gridMain.Columns["option"].OptionsColumn.AllowEdit = true;
        }
        /***********************************************************************************************/
        private void pictureDelete_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            if (dt.Rows.Count <= 0)
                return;
            DataRow dr = gridMain.GetFocusedDataRow();
            if (dr == null)
                return;
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            string record = dr["record"].ObjToString();
            if (String.IsNullOrWhiteSpace(record))
                return;
            if ( record == "-1")
            {
                dt.Rows.RemoveAt(row);
                G1.NumberDataTable(dt);
                dgv.DataSource = dt;
                dgv.Refresh();
                return;
            }
            try
            {
                G1.delete_db_table("funcommoptions", "record", record);
                dt.Rows.RemoveAt(row);
                G1.NumberDataTable(dt);
                dgv.DataSource = dt;
                dgv.Refresh();
            }
            catch ( Exception ex )
            {
            }
        }
        /***********************************************************************************************/
        private void picRowUp_Click(object sender, EventArgs e)
        {
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
            btnSave.Show();
            modified = true;
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
            btnSave.Show();
            modified = true;
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
        /***********************************************************************************************/
        private void btnInsert_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            if (dt.Rows.Count <= 0)
                return;
            DataRow dr = gridMain.GetFocusedDataRow();
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            if (row < 0 || row > (dt.Rows.Count - 1))
                return;
            if (rowHandle == (dt.Rows.Count - 1))
                return; // Already at the last row
            DataRow dRow = dt.NewRow();
            dRow["option"] = "New Option";
            dRow["mod"] = "Y";
            dRow["record"] = -1;
            dt.Rows.InsertAt(dRow, row);
            G1.NumberDataTable(dt);
            dt.AcceptChanges();
            dgv.DataSource = dt;
            gridMain.ClearSelection();
            gridMain.RefreshData();
            gridMain.FocusedRowHandle = rowHandle + 1;
            gridMain.SelectRow(rowHandle + 1);
            dgv.Refresh();
        }
        /***********************************************************************************************/
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        /***********************************************************************************************/
        private int pageMarginLeft = 0;
        private int pageMarginRight = 0;
        private int pageMarginTop = 0;
        private int pageMarginBottom = 0;
        /***********************************************************************************************/
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

            printableComponentLink1.EnablePageDialog = true;

            printableComponentLink1.CreateDetailHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateDetailHeaderArea);
            printableComponentLink1.CreateMarginalHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateMarginalHeaderArea);
            printableComponentLink1.BeforeCreateAreas += new System.EventHandler(this.printableComponentLink1_BeforeCreateAreas);
            printableComponentLink1.AfterCreateAreas += new System.EventHandler(this.printableComponentLink1_AfterCreateAreas);
            printableComponentLink1.Landscape = true;

            //            Printer.setupPrinterMargins(50, 50, 80, 50);
            Printer.setupPrinterMargins(50, 50, 80, 50);

            pageMarginLeft = Printer.pageMarginLeft;
            pageMarginRight = Printer.pageMarginRight;
            pageMarginTop = Printer.pageMarginTop;
            pageMarginBottom = Printer.pageMarginBottom;

            printableComponentLink1.Margins.Left = pageMarginLeft;
            printableComponentLink1.Margins.Right = pageMarginRight;
            printableComponentLink1.Margins.Top = pageMarginTop;
            printableComponentLink1.Margins.Bottom = pageMarginBottom;

            printingSystem1.Document.AutoFitToPagesWidth = 1;

            printableComponentLink1.CreateDocument();
            if (workBatch )
            {
                string filename = @"c:/rag/Manual.pdf";
                if (File.Exists(filename))
                {
                    File.SetAttributes(filename, FileAttributes.Normal);
                    File.Delete(filename);
                }
                printableComponentLink1.ExportToPdf(filename);
            }
            else
                printableComponentLink1.ShowPreview();
        }
        /***********************************************************************************************/
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

            printingSystem1.Document.AutoFitToPagesWidth = 1;

            printableComponentLink1.CreateDocument();
            printableComponentLink1.PrintDlg();
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

            font = new Font("Ariel", 10, FontStyle.Regular);
            string title = this.Text;
            Printer.DrawQuad(5, 8, 4, 4, title, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            //            Printer.DrawQuadTicks();
            DateTime date = DateTime.Now;
            string workDate = date.Month.ToString("D2") + "/" + date.Year.ToString("D4");
            Printer.SetQuadSize(24, 12);
            font = new Font("Ariel", 9, FontStyle.Regular);

            //Printer.DrawQuad(20, 8, 5, 4, "Report Month:" + workDate, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(16, 8, 3, 4, lblPayment.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(19, 8, 3, 4, lblTrust85.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(22, 8, 3, 4, lblTrust100.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            Printer.SetQuadSize(12, 12);
            Printer.DrawQuadBorder(1, 1, 12, 12, BorderSide.All, 1, Color.Black);
            Printer.DrawQuadBorder(12, 1, 1, 12, BorderSide.Right, 1, Color.Black);
        }
        /***************************************************************************************/
        public delegate void d_void_FunCommissionClosing( DataTable dt, string manager, string arranger, string who, double commission );
        public event d_void_FunCommissionClosing funCommissionClosing;
        protected void OnClosing(DataTable dt, double commission )
        {
            if (workBatch)
                funCommissionClosing?.Invoke(dt, workManager, workArranger, workWho, commission );
        }
        /****************************************************************************************/
        private bool pageBreak = false;
        private void gridMain_BeforePrintRow(object sender, DevExpress.XtraGrid.Views.Printing.CancelPrintRowEventArgs e)
        {
            int rowHandle = e.RowHandle;
            if (gridMain.IsDataRow(rowHandle))
            {
                try
                {
                    DataTable dt = (DataTable)dgv.DataSource;
                    int row = gridMain.GetDataSourceRowIndex(rowHandle);

                    string newPage = dt.Rows[row]["option"].ObjToString();
                    if (newPage.ToUpper() == "BREAK")
                    {
                        pageBreak = true;
                        e.Cancel = true;
                    }
                }
                catch (Exception ex)
                {
                }
            }
        }
        /****************************************************************************************/
        private void gridMain_AfterPrintRow(object sender, DevExpress.XtraGrid.Views.Printing.PrintRowEventArgs e)
        {
            if (e.HasFooter)
            {
                    e.PS.InsertPageBreak(e.Y);
            }

            //if (pageBreak)
            //    e.PS.InsertPageBreak(e.Y);
            //pageBreak = false;
        }
        /***********************************************************************************************/
        private void gridMain_CustomDrawCell(object sender, DevExpress.XtraGrid.Views.Base.RowCellCustomDrawEventArgs e)
        {
        }
        /***********************************************************************************************/
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            G1.ShowHideFindPanel(gridMain);
        }
        /***********************************************************************************************/
    }
}