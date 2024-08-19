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
using System.IO;
using System.Web;
using System.Text.RegularExpressions;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using DevExpress.XtraGrid.Views.Grid;
using System.Drawing.Drawing2D;
using DevExpress.Utils.Drawing;
using DevExpress.Utils;
using DevExpress.XtraEditors.Repository;
using System.Diagnostics.Contracts;
using DevExpress.XtraGrid.Columns;
using System.Configuration;
using DevExpress.XtraPrinting;
using DevExpress.XtraEditors.ViewInfo;
/****************************************************************************************/
namespace SMFS
{
    /****************************************************************************************/
    public partial class ContractSignatures : DevExpress.XtraEditors.XtraForm
    {
        private bool loading = true;
        private bool modified = false;
        private string workContract = "";
        private string workFdName = "";
        private DataTable workDt = null;
        private bool QuitAll = false;
        /****************************************************************************************/
        public ContractSignatures(string contract, string fdName, DataTable dt)
        {
            InitializeComponent();
            workContract = contract;
            workFdName = fdName;
            workDt = dt;
        }
        /****************************************************************************************/
        private void ContractSignatures_Load(object sender, EventArgs e)
        {
            oldWhat = "";

            loading = true;
            //btnExit.Hide();

            LoadData();

            QuitAll = false;

            modified = false;
            loading = false;
        }
        /***********************************************************************************************/
        private void LoadData()
        {
            this.Cursor = Cursors.WaitCursor;

            DataTable dt = BuildSignatureTable(workContract, workFdName );

            if (workDt == null)
            {
                workDt = new DataTable();
                workDt.Columns.Add("num");
                workDt.Columns.Add("name");
                workDt.Columns.Add("relationship");
                workDt.Columns.Add("relationType");
                workDt.Columns.Add("signature", dt.Columns["signature"].DataType);
                workDt.Columns.Add("sort");
            }
            else
            {
                btnExit.Show();
                btnExit.Refresh();
            }

            string prefix = "";
            string firstName = "";
            string middleName = "";
            string lastName = "";
            string suffix = "";

            string nok = "";
            string purchaser = "";

            string name = "";

            DataRow dRow = null;
            DataRow[] dRows = null;

            bool gotNok = false;
            bool gotPurchaser = false;

            string relationType = "";
            string relationship = "";
            string informant = "";
            string sort = "";

            if (!String.IsNullOrWhiteSpace(workFdName))
            {
                dRows = workDt.Select("name='" + workFdName + "'");
                if (dRows.Length <= 0)
                {
                    dRow = workDt.NewRow();
                    dRow["name"] = workFdName;
                    dRow["relationship"] = "Funeral Director";
                    dRow["relationType"] = "FD";
                    dRow["sort"] = "0";
                    dRow["signature"] = null;
                    workDt.Rows.Add(dRow);
                }
            }

            if ( workDt != null )
            {
                for (int i = 0; i < workDt.Rows.Count; i++)
                {
                    sort = workDt.Rows[i]["sort"].ObjToString();
                    if (sort == "0")
                        continue;
                    workDt.Rows[i]["sort"] = "999";
                }
            }

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                sort = dt.Rows[i]["sort"].ObjToString();
                if (sort == "0")
                    continue;
                relationship = dt.Rows[i]["relationship"].ObjToString();
                name = dt.Rows[i]["name"].ObjToString();

                relationType = dt.Rows[i]["relationType"].ObjToString();

                if (workDt != null)
                {
                    dRows = workDt.Select("name='" + name + "'");
                    if (dRows.Length > 0)
                    {
                        dRows[0]["relationship"] = relationship;
                        dRows[0]["relationType"] = relationType;
                        dRows[0]["sort"] = sort;
                        //dRows[0]["signature"] = null;
                    }
                    else
                    {
                        dRow = workDt.NewRow();
                        dRow["name"] = name;
                        dRow["relationship"] = relationship;
                        dRow["relationType"] = relationType;
                        dRow["sort"] = sort;
                        dRow["signature"] = null;
                        workDt.Rows.Add(dRow);
                    }
                }
                else
                {
                    dRow = workDt.NewRow();
                    dRow["name"] = name;
                    dRow["relationship"] = relationship;
                    dRow["relationType"] = relationType;
                    dRow["sort"] = sort;
                    dRow["signature"] = null;
                    workDt.Rows.Add(dRow);
                }
            }

            for ( int i=workDt.Rows.Count-1; i>=0; i--)
            {
                sort = workDt.Rows[i]["sort"].ObjToString();
                if (sort == "999")
                    workDt.Rows.RemoveAt(i);
            }

            G1.NumberDataTable(workDt);

            dgv.DataSource = workDt;

            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        public static DataTable BuildSignatureTable(string workContract, string workFdName, DataTable workDt = null)
        {
            string cmd = "Select * from `relatives` WHERE `contractNumber` = '" + workContract + "' ";
            cmd += " AND `depRelationship` <> 'DISCLOSURES' ";
            cmd += " AND `depRelationship` <> 'CLERGY' ";
            cmd += " AND `depRelationship` <> 'PB' ";
            cmd += " AND `depRelationship` <> 'HPB' ";
            cmd += " AND `depRelationship` <> 'MUSICIAN' ";
            cmd += " AND `depRelationship` <> 'FUNERAL DIRECTOR' ";
            cmd += " AND `depRelationship` <> 'PALLBEARER' ";
            cmd += " ORDER by `order`, `depLastName`,`depFirstName`,`depMI` ";

            DataTable dt = G1.get_db_data(cmd);
            dt.Columns.Add("relationType");
            dt.Columns.Add("sort");
            if (G1.get_column_number(dt, "name") < 0)
                dt.Columns.Add("name");
            dt.Columns.Add("primary");


            string prefix = "";
            string firstName = "";
            string middleName = "";
            string lastName = "";
            string suffix = "";

            string nok = "";
            string purchaser = "";

            string name = "";

            DataRow dRow = null;
            DataRow[] dRows = null;

            bool gotNok = false;
            bool gotPurchaser = false;

            string relationType = "";
            string relationship = "";
            string informant = "";
            string sort = "";

            Image fdSig = new Bitmap(1, 1);
            ImageConverter converter = new ImageConverter();
            byte [] blankBytes = (byte[])converter.ConvertTo(fdSig, typeof(byte[]));


            if (!String.IsNullOrWhiteSpace(workFdName))
            {
                dRow = dt.NewRow();
                dRow["name"] = workFdName;
                dRow["depRelationship"] = "Funeral Director";
                dRow["relationType"] = "FD";
                dRow["sort"] = "0";
                dt.Rows.Add(dRow);
            }


            bool gotParent = false;
            bool gotSpouse = false;
            bool gotChild = false;
            bool gotGrands = false;
            bool gotSibling = false;

            DateTime dob = DateTime.Now;
            DateTime dod = DateTime.Now;
            string deceased = "";
            int age = 0;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                nok = dt.Rows[i]["nextOfKin"].ObjToString();
                purchaser = dt.Rows[i]["purchaser"].ObjToString();
                if (nok == "1" || purchaser == "1")
                    dt.Rows[i]["primary"] = "0";
                else
                    dt.Rows[i]["primary"] = "999";
            }

            DataView tempview = dt.DefaultView;
            tempview.Sort = "primary asc";
            dt = tempview.ToTable();


            for (int i = 0; i < dt.Rows.Count; i++)
            {
                relationship = dt.Rows[i]["depRelationship"].ObjToString().ToUpper();
                prefix = dt.Rows[i]["depPrefix"].ObjToString();
                firstName = dt.Rows[i]["depFirstName"].ObjToString();
                middleName = dt.Rows[i]["depMI"].ObjToString();
                lastName = dt.Rows[i]["depLastName"].ObjToString();
                suffix = dt.Rows[i]["depSuffix"].ObjToString();
                name = G1.BuildFullName(prefix, firstName, middleName, lastName, suffix);

                deceased = dt.Rows[i]["deceased"].ObjToString();
                if ( deceased == "1" )
                {
                    dt.Rows[i]["sort"] = "999";
                    continue;
                }
                if ( relationship.IndexOf ( "STEP" ) >= 0 )
                {
                    dt.Rows[i]["sort"] = "999";
                    continue;
                }
                if (relationship.IndexOf("IN-LAW") >= 0)
                {
                    dt.Rows[i]["sort"] = "999";
                    continue;
                }

                dob = dt.Rows[i]["depDOB"].ObjToDateTime();
                age = 0;
                if ( dob.Year > 1000 )
                    age = G1.GetAge(dob, DateTime.Now);

                nok = dt.Rows[i]["nextOfKin"].ObjToString();
                purchaser = dt.Rows[i]["purchaser"].ObjToString();
                informant = dt.Rows[i]["informant"].ObjToString();

                if (nok == "1" || purchaser == "1")
                {
                    relationship = relationship.ToUpper();
                    if (relationship == "HUSBAND" || relationship == "WIFE")
                        gotSpouse = true;
                    else if (relationship == "FATHER" || relationship == "MOTHER")
                        gotParent = true;
                    else if (relationship == "BROTHER" || relationship == "SISTER")
                        gotSibling = true;
                    else if (relationship == "SON" || relationship == "DAUGHTER" || relationship == "CHILD")
                        gotChild = true;
                    else if (relationship == "GRANDSON" || relationship == "GRANDDAUGHTER" || relationship.IndexOf("GRANDCHILD") >= 0)
                        gotGrands = true;

                    if (nok == "1")
                    {
                        if (purchaser == "1")
                            relationType = "NOK-Purchaser";
                        else
                            relationType = "NOK";
                    }
                    else if (purchaser == "1")
                        relationType = "Co-Purchaser";

                    sort = "";
                    if (relationType.IndexOf("NOK") >= 0)
                        sort = "1";
                    else if (relationType.IndexOf("Co-Purchaser") == 0)
                        sort = "6" + i.ToString();
                }
                else
                {
                    relationship = relationship.ToUpper();
                    if (relationship == "HUSBAND" || relationship == "WIFE")
                        relationship = "SPOUSE";
                    else if (relationship == "FATHER" || relationship == "MOTHER")
                        relationship = "PARENT";
                    else if (relationship == "BROTHER" || relationship == "SISTER")
                        relationship = "SIBLING";
                    else if (relationship == "GRANDSON" || relationship == "GRANDDAUGHTER" || relationship.IndexOf("GRANDCHILD") >= 0)
                    {
                        if ( age <= 0 )
                        {
                            dt.Rows[i]["sort"] = "999";
                            continue;
                        }
                        relationship = "GRANDCHILD";
                    }
                    if ( relationship == "SPOUSE" && gotSpouse )
                    {
                        relationType = "NOK";
                        sort = "1" + i.ToString();
                    }
                    else if ( relationship == "SON" || relationship == "DAUGHTER" || relationship == "CHILD" )
                    {
                        if (gotChild)
                        {
                            relationType = "Co-Equal NOK";
                            sort = "2" + i.ToString();
                        }
                    }
                    else if (relationship == "GRANDCHILD" && gotGrands )
                    {
                        relationType = "Co-Equal NOK";
                        sort = "3" + i.ToString();
                    }
                    else if (relationship == "PARENT" && gotParent )
                    {
                        relationType = "Co-Equal NOK";
                        sort = "4" + i.ToString();
                    }
                    else if (relationship == "SIBLING" && gotSibling )
                    {
                        relationType = "Co-Equal NOK";
                        sort = "5" + i.ToString();
                    }
                    else
                        sort = "999";
                }
                dt.Rows[i]["sort"] = sort;
                dt.Rows[i]["name"] = name;
                dt.Rows[i]["relationType"] = relationType;
            }

            tempview = dt.DefaultView;
            tempview.Sort = "sort asc";
            dt = tempview.ToTable();

            for (int i = dt.Rows.Count - 1; i >= 0; i--)
            {
                sort = dt.Rows[i]["sort"].ObjToString();
                if (sort == "999")
                    dt.Rows.RemoveAt(i);
            }

            gotPurchaser = false;
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                relationType = dt.Rows[i]["relationType"].ObjToString();
                if (relationType == "NOK-Purchaser")
                    gotPurchaser = true;
                if ( relationType.IndexOf ( "Co-P") >= 0 )
                {
                    if (!gotPurchaser)
                    {
                        dt.Rows[i]["relationType"] = "Purchaser";
                        gotPurchaser = true;
                    }
                }
            }

            bool initiaizeSignature = false;
            if (workDt == null)
            {
                workDt = new DataTable();
                workDt.Columns.Add("num");
                workDt.Columns.Add("name");
                workDt.Columns.Add("relationship");
                workDt.Columns.Add("relationType");
                workDt.Columns.Add("signature", dt.Columns["signature"].DataType);
                workDt.Columns.Add("sort");
                initiaizeSignature = true;
            }

            if (!String.IsNullOrWhiteSpace(workFdName))
            {
                dRows = workDt.Select("name='" + workFdName + "'");
                if (dRows.Length <= 0)
                {
                    dRow = workDt.NewRow();
                    dRow["name"] = workFdName;
                    dRow["relationship"] = "Funeral Director";
                    dRow["relationType"] = "FD";
                    dRow["sort"] = "0";
                    dRow["signature"] = blankBytes;

                    workDt.Rows.Add(dRow);
                }
            }

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                sort = dt.Rows[i]["sort"].ObjToString();
                if (sort == "0")
                    continue;
                relationship = dt.Rows[i]["depRelationship"].ObjToString();
                prefix = dt.Rows[i]["depPrefix"].ObjToString();
                firstName = dt.Rows[i]["depFirstName"].ObjToString();
                middleName = dt.Rows[i]["depMI"].ObjToString();
                lastName = dt.Rows[i]["depLastName"].ObjToString();
                suffix = dt.Rows[i]["depSuffix"].ObjToString();
                name = G1.BuildFullName(prefix, firstName, middleName, lastName, suffix);

                nok = dt.Rows[i]["nextOfKin"].ObjToString();
                purchaser = dt.Rows[i]["purchaser"].ObjToString();
                informant = dt.Rows[i]["informant"].ObjToString();
                relationType = dt.Rows[i]["relationType"].ObjToString();


                dRows = workDt.Select("name='" + name + "'");
                if (dRows.Length > 0)
                {
                    dRows[0]["relationship"] = dt.Rows[i]["depRelationship"].ObjToString();
                    dRows[0]["relationType"] = relationType;
                    dRows[0]["sort"] = sort;
                }
                else
                {
                    dRow = workDt.NewRow();
                    dRow["name"] = name;
                    dRow["relationship"] = dt.Rows[i]["depRelationship"].ObjToString();
                    dRow["relationType"] = relationType;
                    dRow["sort"] = sort;
                    dRow["signature"] = blankBytes;
                    workDt.Rows.Add(dRow);
                }
            }

            return workDt;
        }
        /***********************************************************************************************/
        private void LoadDatax()
        {
            this.Cursor = Cursors.WaitCursor;

            string cmd = "Select * from `relatives` WHERE `contractNumber` = '" + workContract + "' ";
            cmd += " AND `depRelationship` <> 'DISCLOSURES' ";
            cmd += " AND `depRelationship` <> 'CLERGY' ";
            cmd += " AND `depRelationship` <> 'PB' ";
            cmd += " AND `depRelationship` <> 'HPB' ";
            cmd += " AND `depRelationship` <> 'MUSICIAN' ";
            cmd += " AND `depRelationship` <> 'FUNERAL DIRECTOR' ";
            cmd += " AND `depRelationship` <> 'PALLBEARER' ";
            cmd += " ORDER by `depLastName`,`depFirstName`,`depMI` ";

            DataTable dt = G1.get_db_data(cmd);
            dt.Columns.Add("relationType");
            dt.Columns.Add("sort");

            if (workDt == null)
            {
                workDt = new DataTable();
                workDt.Columns.Add("num");
                workDt.Columns.Add("name");
                workDt.Columns.Add("relationship");
                workDt.Columns.Add("relationType");
                workDt.Columns.Add("signature", dt.Columns["signature"].DataType);
                workDt.Columns.Add("sort");
            }
            else
            {
                btnExit.Show();
                btnExit.Refresh();
            }

            string prefix = "";
            string firstName = "";
            string middleName = "";
            string lastName = "";
            string suffix = "";

            string nok = "";
            string purchaser = "";

            string name = "";

            DataRow dRow = null;
            DataRow[] dRows = null;

            bool gotNok = false;
            bool gotPurchaser = false;

            string relationType = "";
            string relationship = "";
            string informant = "";
            string sort = "";

            if (!String.IsNullOrWhiteSpace(workFdName))
            {
                dRows = workDt.Select("name='" + workFdName + "'");
                if (dRows.Length <= 0)
                {
                    dRow = workDt.NewRow();
                    dRow["name"] = workFdName;
                    dRow["relationship"] = "Funeral Director";
                    dRow["relationType"] = "FD";
                    dRow["sort"] = "0";
                    workDt.Rows.Add(dRow);
                }
            }

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                relationship = dt.Rows[i]["depRelationship"].ObjToString();
                prefix = dt.Rows[i]["depPrefix"].ObjToString();
                firstName = dt.Rows[i]["depFirstName"].ObjToString();
                middleName = dt.Rows[i]["depMI"].ObjToString();
                lastName = dt.Rows[i]["depLastName"].ObjToString();
                suffix = dt.Rows[i]["depSuffix"].ObjToString();
                name = G1.BuildFullName(prefix, firstName, middleName, lastName, suffix);

                nok = dt.Rows[i]["nextOfKin"].ObjToString();
                purchaser = dt.Rows[i]["purchaser"].ObjToString();
                informant = dt.Rows[i]["informant"].ObjToString();

                if (nok == "1" || purchaser == "1")
                {
                    if (nok == "1")
                    {
                        if (purchaser == "1")
                            relationType = "NOK-Purchaser";
                        else
                            relationType = "NOK";
                    }
                    else if (purchaser == "1")
                    {
                        if (gotPurchaser)
                            relationType = "Co-Purchaser";
                        else
                            relationType = "Purchaser";
                    }

                    sort = "";
                    if (relationType.IndexOf("NOK") >= 0)
                        sort = "1";
                    else if (relationType.IndexOf("Purchaser") == 0)
                    {
                        sort = "3";
                    }
                    else if (relationType.IndexOf("Co-Purchaser") == 0)
                    {
                        sort = "4";
                    }

                    if (nok == "1")
                        gotNok = true;
                    if (purchaser == "1")
                        gotPurchaser = true;

                    dRows = workDt.Select("name='" + name + "'");
                    if (dRows.Length > 0)
                    {
                        dRows[0]["relationship"] = dt.Rows[i]["depRelationship"].ObjToString();
                        dRows[0]["relationType"] = relationType;
                        dRows[0]["sort"] = sort;
                    }
                    else
                    {
                        dRow = workDt.NewRow();
                        dRow["name"] = name;
                        dRow["relationship"] = dt.Rows[i]["depRelationship"].ObjToString();
                        dRow["relationType"] = relationType;
                        dRow["sort"] = sort;
                        workDt.Rows.Add(dRow);
                    }
                }
                else
                {
                    sort = "6";
                    dRows = workDt.Select("name='" + name + "'");
                    if (dRows.Length > 0)
                    {
                        dRows[0]["relationship"] = dt.Rows[i]["depRelationship"].ObjToString();
                        dRows[0]["relationType"] = "Co-Equal Class";
                        dRows[0]["sort"] = "999";
                    }
                }
            }

            DataView tempview = workDt.DefaultView;
            tempview.Sort = "sort asc";
            workDt = tempview.ToTable();

            for (int i = workDt.Rows.Count - 1; i >= 0; i--)
            {
                sort = workDt.Rows[i]["sort"].ObjToString();
                if (sort == "999")
                    workDt.Rows.RemoveAt(i);
            }

            G1.NumberDataTable(workDt);

            dgv.DataSource = workDt;

            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void gridMain_CustomRowFilter(object sender, DevExpress.XtraGrid.Views.Base.RowFilterEventArgs e)
        {
            int row = e.ListSourceRow;
            if (dgv == null)
                return;
            if (dgv.DataSource == null)
                return;
            DataTable dt = (DataTable)dgv.DataSource;
            if (dt.Rows.Count <= 0)
                return;
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
                return;
            }
            int rowHandle = e.RowHandle;
            if (rowHandle < 0)
                return;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            if (dgv.DataSource == null)
                return;
        }
        /****************************************************************************************/
        private bool justSaved = false;
        private void gridMain_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            DataRow dr = gridMain.GetFocusedDataRow();
            if (dr == null)
                return;

            //dr["mod"] = "Y";
            //GridColumn currCol = gridMain.FocusedColumn;
            //string currentColumn = currCol.FieldName;
            //if (currentColumn.ToUpper() == "NUM")
            //    return;

            string what = dr[currentColumn].ObjToString();
            gridMain.RefreshData();
        }
        /****************************************************************************************/
        private void UpdateMod(DataRow dr)
        {
            dr["mod"] = "Y";
            modified = true;
            btnExit.Show();
            btnExit.Refresh();
        }
        /***********************************************************************************************/
        private void AddMod(DataTable dt, DevExpress.XtraGrid.Views.Grid.GridView grid)
        {
            if (G1.get_column_number(dt, "mod") < 0)
                dt.Columns.Add("mod");
        }
        /****************************************************************************************/
        private void pictureBox4_Click(object sender, EventArgs e)
        {
            if (gridMain.OptionsFind.AlwaysVisible == true)
                gridMain.OptionsFind.AlwaysVisible = false;
            else
                gridMain.OptionsFind.AlwaysVisible = true;
        }
        /****************************************************************************************/
        private void gridMain_CustomColumnDisplayText(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventArgs e)
        {
            if (e.ListSourceRowIndex == DevExpress.XtraGrid.GridControl.InvalidRowHandle)
                return;
            string name = e.Column.FieldName;
            if ( name.ToUpper().IndexOf("DATE") >= 0 )
            {
                if (e.DisplayText.IndexOf("0000") >= 0 || e.DisplayText.IndexOf("0001") >= 0)
                    e.DisplayText = "";
                else
                {
                    DateTime date = e.DisplayText.ObjToString().ObjToDateTime();
                    e.DisplayText = date.ToString("MM/dd/yyyy");
                    if (date.Year < 30)
                        e.DisplayText = "";
                }
            }
        }
        /****************************************************************************************/
        public DataTable SignatureResults
        {
            get
            {
                return workDt;
            }
        }
        /****************************************************************************************/
        public bool ExitAll
        {
            get
            {
                return QuitAll;
            }
        }
        /****************************************************************************************/
        private void gridMain_DoubleClick(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            DataTable dt = (DataTable)dgv.DataSource;
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetFocusedDataSourceRowIndex();
            string what = dr["relationType"].ObjToString();

            Image emptyImage = new Bitmap(1, 1);
            Byte[] bytes = dt.Rows[row]["signature"].ObjToBytes();
            Image myImage = emptyImage;
            if (bytes != null)
                myImage = G1.byteArrayToImage(bytes);

            Image signature = GetSignature("Enter Signature of " + what, myImage );
            if ( signature != null )
            {
                ImageConverter converter = new ImageConverter();
                bytes = (byte[])converter.ConvertTo(signature, typeof(byte[]));
                dr["signature"] = bytes;
                gridMain.RefreshData();
                gridMain.RefreshEditor(true);
                dgv.Refresh();
                modified = true;
                btnExit.Show();
                btnExit.Refresh();
            }
        }
        /****************************************************************************************/
        private Image GetSignature(string what, Image signature )
        {
            //Image signature = new Bitmap(1, 1);
            using (SignatureForm signatureForm = new SignatureForm(what, signature))
            {
                if (signatureForm.ShowDialog() == DialogResult.OK)
                {
                    signature = signatureForm.SignatureResult;
                }
                else
                    signature = null;
            }
            return signature;
        }
        /****************************************************************************************/
        private void gridMain_FocusedRowChanged(object sender, DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            DataRow dr = gridMain.GetFocusedDataRow();

            int focusedRow = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(focusedRow);

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
            //            Printer.DrawQuad(6, 8, 4, 4, "Funeral Services Report", Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            Printer.DrawQuad(5, 8, 8, 4, this.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

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
        /****************************************************************************************/
        private void gridMain_ValidatingEditor(object sender, DevExpress.XtraEditors.Controls.BaseContainerValidateEditorEventArgs e)
        {
            GridView view = sender as GridView;
        }
        private string oldWhat = "";
        /****************************************************************************************/
        private void gridMain_CustomRowCellEdit(object sender, CustomRowCellEditEventArgs e)
        {
            GridView view = sender as GridView;
            //if (e.Column.FieldName.ToUpper() == "CHECKLIST")
            //{
            //    string type = view.GetRowCellValue(e.RowHandle, "type").ObjToString().ToUpper();
            //    if (type != "INSURANCE" && type != "POLICY" && type != "INSURANCE DIRECT" && type != "INSURANCE UNITY" && type != "3RD PARTY")
            //    {
            //        e.RepositoryItem = null;
            //        return;
            //    }
            //    string status = view.GetRowCellValue(e.RowHandle, "status").ObjToString();
            //    if (status.ToUpper() == "FILED")
            //        e.RepositoryItem = this.repositoryItemButtonEdit2;
            //    else if ( status.ToUpper() == "DEPOSITED")
            //        e.RepositoryItem = this.repositoryItemButtonEdit1;
            //    else
            //        e.RepositoryItem = this.repositoryItemButtonEdit2;
            //}
        }
        /****************************************************************************************/
        private string oldColumn = "";
        private DataTable trackingDt = null;
        private DataTable trackDt = null;
        RepositoryItemComboBox ciLookup = new RepositoryItemComboBox();
        /****************************************************************************************/
        private void gridMain_ShownEditor(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            int row = gridMain.FocusedRowHandle;

            GridColumn currCol = gridMain.FocusedColumn;
            DataRow dr = gridMain.GetFocusedDataRow();
            string name = currCol.FieldName;
            string record = "";
            string str = "";
            DateTime myDate = DateTime.Now;
            oldColumn = name;

            bool doDate = false;
            //if (name == "apptDate")
            //    doDate = true;
            //else if (name == "lastContactDate")
            //    doDate = true;

            if (name.ToUpper().IndexOf("DATE") >= 0)
                doDate = true;

            if (doDate)
            {
                myDate = dr[name].ObjToDateTime();
                str = gridMain.Columns[name].Caption;
                using (GetDate dateForm = new GetDate(myDate, str))
                {
                    dateForm.ShowDialog();
                    if (dateForm.DialogResult == System.Windows.Forms.DialogResult.OK)
                    {
                        try
                        {
                            myDate = dateForm.myDateAnswer;
                            DateTime date = myDate.ObjToDateTime();

                            dr[name] = G1.DTtoMySQLDT(myDate);
                        }
                        catch (Exception ex)
                        {
                        }
                        //dr[name] = G1.DTtoMySQLDT(myDate);
                        UpdateMod(dr);
                        gridMain_CellValueChanged(null, null);
                    }
                }
            }
            gridMain.RefreshData();
            gridMain.RefreshEditor(true);
        }
        /****************************************************************************************/
        private string currentColumn = "";
        private string oldContactType = "";
        /****************************************************************************************/
        private void comboBox_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            System.Windows.Forms.ComboBox cbo = (System.Windows.Forms.ComboBox)sender;
            cbo.PreviewKeyDown -= comboBox_PreviewKeyDown;
            if (cbo.DroppedDown) cbo.Focus();
        }
        /****************************************************************************************/
        private void gridMain_CalcRowHeight(object sender, RowHeightEventArgs e)
        {
            GridView View = sender as GridView;
            if (e.RowHandle >= 0)
            {
                int maxHeight = 0;
                int newHeight = 0;
                bool doit = false;
                string name = "";
                string str = "";
                int count = 0;
                string[] Lines = null;
                foreach (GridColumn column in gridMain.Columns)
                {
                    name = column.FieldName.ToUpper();
                    //if (name == "RESULTS" )
                    //    doit = true;
                    if (doit)
                    {
                        using (RepositoryItemMemoEdit edit = new RepositoryItemMemoEdit())
                        {
                            using (MemoEditViewInfo viewInfo = edit.CreateViewInfo() as MemoEditViewInfo)
                            {
                                str = gridMain.GetRowCellValue(e.RowHandle, column.FieldName).ObjToString();
                                if ( !String.IsNullOrWhiteSpace ( str ))
                                {
                                    Lines = str.Split('\n');
                                    count = Lines.Length + 1;
                                }
                                viewInfo.EditValue = gridMain.GetRowCellValue(e.RowHandle, column.FieldName);
                                viewInfo.Bounds = new Rectangle(0, 0, column.VisibleWidth, dgv.Height);
                                using (Graphics graphics = dgv.CreateGraphics())
                                using (GraphicsCache cache = new GraphicsCache(graphics))
                                {
                                    viewInfo.CalcViewInfo(graphics);
                                    var height = ((IHeightAdaptable)viewInfo).CalcHeight(cache, column.VisibleWidth);
                                    newHeight = Math.Max(height, maxHeight);
                                    if (newHeight > maxHeight)
                                    {
                                        maxHeight = newHeight * count;
                                    }
                                }
                            }
                        }
                    }
                }

                if (maxHeight > 0 && maxHeight > e.RowHeight )
                    e.RowHeight = maxHeight;
                e.RowHeight = 40;
            }
        }
        /****************************************************************************************/
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            modified = false;
            this.Close();
        }
        /****************************************************************************************/
        private void btnExit_Click(object sender, EventArgs e)
        {
            modified = false;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
        /****************************************************************************************/
        private void ContractSignatures_FormClosing(object sender, FormClosingEventArgs e)
        {
            if ( modified )
            {
                DialogResult result = MessageBox.Show("***Question***\nData has been modified!\nWould you like a to save your changes?", "Data Modified Dialog", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                if (result == DialogResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }
                if (result == DialogResult.Yes)
                    btnExit_Click(null, null);
                modified = false;
            }
            this.Validate();
            gridMain.RefreshEditor(true);
        }
        /****************************************************************************************/
        private void btnExitAll_Click(object sender, EventArgs e)
        {
            modified = false;
            QuitAll = true;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
        /****************************************************************************************/
    }
}