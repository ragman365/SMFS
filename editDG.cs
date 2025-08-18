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
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraEditors.ViewInfo;
using DevExpress.Utils.Drawing;
using System.Web.UI.WebControls;
using System.Runtime.Remoting.Contexts;
using DevExpress.Utils;
using System.Web;
using System.IO;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using DevExpress.XtraEditors.Popup;
using DevExpress.Utils.Win;
/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public partial class editDG : DevExpress.XtraEditors.XtraForm
    {
        /***********************************************************************************************/
        private DevExpress.XtraGrid.Views.Grid.GridView workGV = null;
        private DataTable workDt = null;
        private int workRow = -1;
        private string workRecord = "";
        private int notesRow = -1;
        private int resultsRow = -1;
        private bool workSecondary = false;
        private bool modified = false;
        /***********************************************************************************************/
        public editDG(DevExpress.XtraGrid.Views.Grid.GridView gv, DataTable dt, int row, string record, bool secondary = false  )
        {
            InitializeComponent();
            workGV = gv;
            workDt = dt;
            workRow = row;
            workRecord = record;
            workSecondary = secondary;
        }
        /***********************************************************************************************/
        RepositoryItemComboBox ciLookup = new RepositoryItemComboBox();
        private DataTable myDt = null;
        private string currentColumn = "";

        private int repositoryCount = 0;
        private string[] repositoryNames = new string[10];
        private string[] repositoryCaptions = new string[10];
        private RepositoryItemComboBox[] Repository = new RepositoryItemComboBox[10];
        /***********************************************************************************************/
        private void editDG_Load(object sender, EventArgs e)
        {
            btnAccept.Hide();
            btnCancel.Hide();

            textBox1.Hide();
            comboBox1.Hide();

            //gridMain.Columns["data"].ColumnEdit.AllowHtmlDraw = DefaultBoolean.False;

            //gridMain.FilterPopupExcelData += GridMain_FilterPopupExcelData;
            //repositoryItemMemoEdit1.AllowHtmlDraw = DefaultBoolean.False;

            if ( workSecondary )
            {
                gridMain.Columns["num"].Width = 45;
                gridMain.Columns["field"].Width = 200;
                //gridMain.OptionsBehavior.ReadOnly = true;
                //panelTop.Hide();
                this.Dock = DockStyle.Fill;
            }
            else
            {
                gridMain.Columns["field"].OptionsColumn.AllowEdit = false;
            }


            if (myDt == null)
            {
                myDt = new DataTable();
                myDt.Columns.Add("stuff");
            }
            myDt.Rows.Clear();

            string caption = "";
            string field = "";
            string data = "";

            DataRow dRow = null;
            DataTable newDt = new DataTable();
            newDt.Columns.Add("field");
            newDt.Columns.Add("data");
            newDt.Columns.Add("actualField");

            string columnEdit = "";
            int idx = 0;

            DataTable myList = new DataTable();
            myList.Columns.Add("field");
            myList.Columns.Add("sort");

            for (int i = 0; i < workGV.Columns.Count; i++)
            {
                caption = workGV.Columns[i].Caption;
                if ( caption == "Next Touch Date")
                {
                }
                field = workGV.Columns[i].FieldName;
                if (workSecondary)
                {
                    if (field.ToUpper() == "NOTES")
                        continue;
                    if (field.ToUpper().IndexOf("RESULTS") >= 0)
                        continue;
                }
                idx = workGV.Columns[i].VisibleIndex;
                if (idx < 0)
                    continue;
                if (!workGV.Columns[i].Visible)
                    continue;
                dRow = myList.NewRow();
                dRow["field"] = field;
                dRow["sort"] = idx.ToString("D3");
                myList.Rows.Add(dRow);
            }

            DataView tempview = myList.DefaultView;
            tempview.Sort = "sort asc";
            myList = tempview.ToTable();

            for ( int i=0; i<myList.Rows.Count; i++)
            {
                field = myList.Rows[i]["field"].ObjToString();
                if ( field.ToUpper() == "NOTES")
                {
                }
                if (!workGV.Columns[field].Visible)
                    continue;
                if (G1.get_column_number(workDt, field) < 0)
                    continue;
                if (field.ToUpper() == "NUM")
                    continue;
                caption = workGV.Columns[field].Caption;
                data = workDt.Rows[workRow][field].ObjToString();

                columnEdit = workGV.Columns[field].ColumnEditName.ObjToString();
                //if (columnEdit.ToUpper().IndexOf("MEMOEDIT") > 0)
                //{
                //    continue;
                //}
                if (columnEdit.ToUpper().IndexOf("CHECKEDIT") > 0)
                    continue;
                if (!String.IsNullOrWhiteSpace(columnEdit) && columnEdit.ToUpper().IndexOf("MEMOEDIT") < 0 )
                {
                    //FunPayments form = (FunPayments)G1.IsFormOpen("FunPayments", workContract);

                    ContactsPreneed form = (ContactsPreneed)G1.IsFormOpen("ContactsPreneed");
                    if (form != null)
                    {
                        RepositoryItemComboBox itemBox = form.FireEventGrabSomething(columnEdit);
                        repositoryCaptions[repositoryCount] = caption;
                        repositoryNames[repositoryCount] = columnEdit;
                        Repository[repositoryCount] = itemBox;
                        repositoryCount++;
                    }
                }

                dRow = newDt.NewRow();
                dRow["field"] = caption;
                dRow["actualField"] = field;
                dRow["data"] = data;
                newDt.Rows.Add(dRow);

                if (caption.ToUpper() == "NOTES")
                    notesRow = newDt.Rows.Count - 1;
                if (caption.ToUpper() == "RESULTS")
                    resultsRow = newDt.Rows.Count - 1;
            }
            G1.NumberDataTable(newDt);

            gridMain.Columns["num"].OptionsColumn.AllowEdit = false;
            gridMain.Columns["num"].OptionsColumn.ReadOnly = true;
            gridMain.Columns["num"].OptionsColumn.TabStop = false;
            gridMain.Columns["field"].OptionsColumn.AllowEdit = false;
            gridMain.Columns["field"].OptionsColumn.ReadOnly = true;
            gridMain.Columns["field"].OptionsColumn.TabStop = false;

            dgv.DataSource = newDt;

            ciLookup.SelectedIndexChanged += CiLookup_SelectedIndexChanged;
            ciLookup.MouseDown += CiLookup_MouseDown;
            ciLookup.EditValueChanged += CiLookup_EditValueChanged;

            ciLookup.BeforePopup += CiLookup_BeforePopup;
            ciLookup.Popup += CiLookup_Popup;

            ciLookup.CloseUp += CiLookup_CloseUp;

        }
        /***************************************************************************************/
        private void CiLookup_CloseUp(object sender, DevExpress.XtraEditors.Controls.CloseUpEventArgs e)
        {
            int count = ciLookup.Items.Count;
            //gridMain.Columns["data"].ColumnEdit = null;
        }
        /***************************************************************************************/
        private PopupListBoxForm popupForm = null;
        private int lastIndex = -1;
        /***************************************************************************************/
        private void CiLookup_Popup(object sender, EventArgs e)
        {
            //popupForm = (sender as IPopupControl).PopupWindow as PopupListBoxForm;
            //popupForm.ListBox.MouseMove += ListBox_MouseMove;
            //popupForm.ListBox.MouseDown += ListBox_MouseDown;
            //popupForm.ListBox.SelectedValueChanged += ListBox_SelectedValueChanged;

            //popupForm.ListBox.Focus();
            //popupForm.ListBox.Show();

            //gridMain.Columns["data"].ColumnEdit = null;

            ComboBoxEdit box = (ComboBoxEdit)sender;
            if (box.Properties.Items.Count > 0)
            {
                System.Windows.Forms.ComboBox newBox = comboBox1;
                ciLookup.Items.Clear();
                for (int i = 0; i < comboBox1.Items.Count; i++)
                {
                    ciLookup.Items.Add(comboBox1.Items[i]);
                }
                gridMain.Columns["data"].ColumnEdit = ciLookup;
                //gridMain.Focus();
            }
        }
        /***************************************************************************************/
        private void ListBox_SelectedValueChanged(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string item = dr["data"].ObjToString();
            item = popupForm.ListBox.SelectedValue.ObjToString();
            //dr["data"] = item;
            //gridMain6.RefreshData();
            //gridMain6.RefreshEditor(true);
            int index = popupForm.ListBox.SelectedIndex;
            if (index == lastIndex)
                return;
        }
        /***************************************************************************************/
        private void CiLookup_BeforePopup(object sender, EventArgs e)
        {
            ComboBoxEdit box = (ComboBoxEdit)sender;
            if ( box.Properties.Items.Count > 0 )
            {
                System.Windows.Forms.ComboBox newBox = comboBox1;
                ciLookup.Items.Clear();
                for ( int i=0; i<comboBox1.Items.Count; i++)
                {
                    ciLookup.Items.Add(comboBox1.Items[i]);
                }
                gridMain.Columns["data"].ColumnEdit = ciLookup;
                gridMain.RefreshEditor(true);
                dgv.Refresh();
            }
        }
        /***************************************************************************************/
        private void CiLookup_EditValueChanged(object sender, EventArgs e)
        {
        }
        /***************************************************************************************/
        private string lastDrop = "";
        private void CiLookup_MouseDown(object sender, MouseEventArgs e)
        {
            //gridMain.Columns["data"].ColumnEdit = null;
            //gridMain.RefreshEditor(true);
            //gridMain.PostEditor();

            GridColumn currCol = gridMain.FocusedColumn;
            currentColumn = currCol.FieldName;


            DataRow dr = gridMain.GetFocusedDataRow();
            string field = dr["field"].ObjToString();

            if (currentColumn.ToUpper() != "DATA")
                return;

            if (String.IsNullOrWhiteSpace(lastDrop))
                lastDrop = field;

            if (lastDrop != field )
            {
                //currCol.ColumnEdit = null;
                //gridMain.Columns["data"].ColumnEdit = null;
                //gridMain.FocusedColumn.ColumnEdit = null;

                //gridMain.RefreshEditor(true);
                //gridMain.PostEditor();
                //gridMain.RefreshData();
            }

            lastDrop = field;

            ciLookup.Items.Clear();
            if (myDt == null)
            {
                myDt = new DataTable();
                myDt.Columns.Add("stuff");
            }
            myDt.Rows.Clear();

            isRepository(field, ref myDt);

            if (myDt.Rows.Count > 0)
            {
                for (int i = 0; i < myDt.Rows.Count; i++)
                    ciLookup.Items.Add(myDt.Rows[i]["stuff"].ObjToString());
                //gridMain.FocusedColumn.ColumnEdit = null;
                gridMain.Columns["data"].ColumnEdit = ciLookup;
                //currCol.ColumnEdit = ciLookup;
                //gridMain.RefreshEditor(true);
                //gridMain.PostEditor();
                //gridMain.RefreshData();
            }
            else
            {
                //currCol.ColumnEdit = null;
            }


            //repositoryItemComboBox1.ShowDropDown = DevExpress.XtraEditors.Controls.ShowDropDown.SingleClick;
            //repositoryItemComboBox1_SelectedIndexChanged(sender, null);
            //int focusedRow = gridMain.FocusedRowHandle;
            //int row = gridMain.GetDataSourceRowIndex(focusedRow);
            //dgv.Select();
            //gridMain.SelectRow(focusedRow);
            //gridMain.FocusedRowHandle = row;
            //gridMain.SelectRow(focusedRow);
            //gridMain.FocusedRowHandle = row;
        }
        /***************************************************************************************/
        private void CiLookup_SelectedIndexChanged(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            DataRow dr = gridMain.GetFocusedDataRow();
            int rowhandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowhandle);

            DevExpress.XtraEditors.ComboBoxEdit box = (DevExpress.XtraEditors.ComboBoxEdit)sender;
            string newData = box.Text;

            dt.Rows[row]["data"] = newData;

            gridMain.Columns["data"].ColumnEdit = null;

            FireEventModified ();
        }
        /***************************************************************************************/
        private void GridMain_FilterPopupExcelData(object sender, DevExpress.XtraGrid.Views.Grid.FilterPopupExcelDataEventArgs e)
        {
        }
        /****************************************************************************************/
        public void FireEventModified()
        {
            btnAccept.Show();
            btnAccept.Refresh();
            btnCancel.Show();
            btnCancel.Refresh();
        }
        /***************************************************************************************/
        public void fireDemoDone ()
        {
            OnDone();
        }
        /***************************************************************************************/
        public delegate void d_void_eventdone_datatable( DataTable dt, int row, string CancelStatus );
        public event d_void_eventdone_datatable editDone;
        protected void OnDone()
        {
            if (editDone != null)
            {
                DataTable dt = (DataTable)dgv.DataSource;
                editDone.Invoke(dt, workRow, CancelButton);
                this.Hide();
                //this.Close();
            }
        }
        /***********************************************************************************************/
        private void editDG_FormClosed(object sender, FormClosedEventArgs e)
        {
             OnDone();
        }
        /***********************************************************************************************/
        private string CancelButton = "";
        private void btnCancel_Click(object sender, EventArgs e)
        {
            CancelButton = "YES";
            this.Close();
        }
        /***********************************************************************************************/
        private void btnAccept_Click(object sender, EventArgs e)
        {
            CancelButton = "Accept";
            //if (workSecondary)
            //{
            //    DataTable dt = (DataTable)dgv.DataSource;
            //    editDone.Invoke(dt, workRow);
            //    modified = true;
            //    // return;
            //}
            this.Close();
        }
        /***********************************************************************************************/
        private bool isRepository ( string field, ref DataTable dt )
        {
            bool gotit = false;
            DataRow dRow = null;
            string item = "";
            for ( int i=0; i<repositoryCount; i++)
            {
                if ( field.ToUpper() == repositoryCaptions[i].Trim().ToUpper() )
                {
                    gotit = true;
                    DevExpress.XtraEditors.Controls.ComboBoxItemCollection box = ( DevExpress.XtraEditors.Controls.ComboBoxItemCollection )Repository[i].Items;
                    for ( int j=0; j<box.Count; j++)
                    {
                        item = box[j].ToString();
                        dRow = dt.NewRow();
                        dRow["stuff"] = item;
                        dt.Rows.Add(dRow);
                    }
                    break;
                }
            }
            if ( !gotit )
            {
            }
            return gotit;
        }
        /***********************************************************************************************/
        private void gridMain_ShownEditor(object sender, EventArgs e)
        {
            GridColumn currCol = gridMain.FocusedColumn;
            currentColumn = currCol.FieldName;

            int focusedRow = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(focusedRow);

            DataRow dr = gridMain.GetFocusedDataRow();
            string field = dr["field"].ObjToString();

            //gridMain.Columns["data"].ColumnEdit = null;

            if (currentColumn.ToUpper() != "DATA")
                return;

            if ( field == "Prospect Creation Date")
            {
            }

            if (textBox1.Text.ToUpper() == field.ToUpper())
                return;

            textBox1.Text = field;
            textBox1.Refresh();

            if (1 == 1)
                return;

            comboBox1.Items.Clear();

            ciLookup.Items.Clear();
            if (myDt == null)
            {
                myDt = new DataTable();
                myDt.Columns.Add("stuff");
            }
            myDt.Rows.Clear();

            isRepository(field, ref myDt);

            if (myDt.Rows.Count > 0)
            {
                for (int i = 0; i < myDt.Rows.Count; i++)
                {
                    ciLookup.Items.Add(myDt.Rows[i]["stuff"].ObjToString());
                    comboBox1.Items.Add (myDt.Rows[i]["stuff"].ObjToString());
                }
                //currCol.ColumnEdit = ciLookup;
                gridMain.Columns["data"].ColumnEdit = ciLookup;

                //dgv.Refresh();

                //gridMain.RefreshEditor(true);
                //gridMain.RefreshData();
                //gridMain.PostEditor();
            }
            else
            {
                gridMain.Columns["data"].ColumnEdit = null;
                //currCol.ColumnEdit = null;
            }
        }
        /***********************************************************************************************/
        private void gridMain_MouseDown(object sender, MouseEventArgs e)
        {
            //if (workSecondary)
            //    return;
            gridMain.PostEditor();
            var hitInfo = gridMain.CalcHitInfo(e.Location);
            if (hitInfo.InRowCell)
            {
                DataRow dr = gridMain.GetFocusedDataRow();
                int rowhandle = gridMain.FocusedRowHandle;
                rowhandle = hitInfo.RowHandle;
                int row = gridMain.GetDataSourceRowIndex(rowhandle);
                DataTable dt = (DataTable)dgv.DataSource;
                string field = dt.Rows[row]["field"].ObjToString();
                string data = dt.Rows[row]["data"].ObjToString();
                //string columnEdit = dr["columnEdit"].ObjToString();
                //if ( !String.IsNullOrWhiteSpace ( columnEdit ))
                //{
                //}
                if (field.ToUpper() != "NOTES" && field.ToUpper() != "RESULTS")
                {
                    bool doDate = false;
                    if (field.ToUpper().IndexOf("DATE") >= 0)
                        doDate = true;
                    else if (field.ToUpper() == "BIRTHDAY" )
                        doDate = true;

                    if (doDate)
                    {
                        DateTime myDate = data.ObjToDateTime();
                        using (GetDate dateForm = new GetDate(myDate, field, 0, true ))
                        {
                            dateForm.ShowDialog();
                            if (dateForm.DialogResult == System.Windows.Forms.DialogResult.OK)
                            {
                                try
                                {
                                    myDate = dateForm.myDateAnswer;
                                    DateTime date = myDate.ObjToDateTime();
                                    dt.Rows[row]["data"] = date.ToString("MM/dd/yyyy");
                                    if (field.ToUpper() == "BIRTHDAY")
                                    {
                                        int age = G1.CalculateAgeCorrect(date, DateTime.Now);
                                        DataRow[] dRows = dt.Select("field='age'");
                                        if (dRows.Length > 0)
                                        {
                                            dRows[0]["data"] = age.ToString();
                                        }
                                    }

                                    btnAccept.Show();
                                    btnAccept.Refresh();
                                    btnCancel.Show();
                                    btnCancel.Refresh();

                                }
                                catch (Exception ex)
                                {
                                }
                            }
                        }
                    }
                    gridMain.RefreshEditor(true);
                    return;
                }
                DateTime today = DateTime.Now;
                if (!String.IsNullOrWhiteSpace(data))
                    data += "\n";
                data += today.ToString("MM/dd/yyyy") + " ";

                using (EditTextData fmrmyform = new EditTextData(field, data))
                {
                    fmrmyform.Text = "";
                    fmrmyform.ShowDialog();
                    if (fmrmyform.DialogResult == DialogResult.OK)
                    {
                        string p = fmrmyform.Answer.Trim();
                        dt.Rows[row]["data"] = p;
                        gridMain.RefreshEditor(true);
                        gridMain.RefreshData();
                    }
                }
            }
        }
        /***********************************************************************************************/
        private void gridMain_CustomColumnDisplayText(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventArgs e)
        {
            if (e.Column.FieldName.ToUpper().IndexOf("DATE") >= 0 && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
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
            else if (e.Column.FieldName.ToUpper() == "DOB" && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
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
        /***********************************************************************************************/
        private void gridMain_CalcRowHeight(object sender, DevExpress.XtraGrid.Views.Grid.RowHeightEventArgs e)
        {
            e.RowHeight = 16;
            GridView View = sender as GridView;
            if (e.RowHandle >= 0)
            {
                DataTable dt = (DataTable)dgv.DataSource;
                if (dt == null)
                    return;
                int maxHeight = 0;
                int newHeight = 0;
                bool doit = false;
                string name = "";
                Font f = gridMain.Appearance.Row.Font;
                int rowHeight = f.Height;
                string str = "";
                int maxLength = 0;
                int length = 0;
                int thisRow = gridMain.GetDataSourceRowIndex(e.RowHandle);
                string field = dt.Rows[thisRow]["field"].ObjToString();
                if (field.ToUpper() != "NOTES" && field.ToUpper() != "RESULTS")
                    return;
                if ( field.ToUpper() == "RESULTS")
                {
                }
                int periods = 0;
                //GridColumn cc = gridMain.Columns["data"];
                GridColumn column = gridMain.Columns["data"];
                {
                    name = field;
                    doit = true;
                    if (doit)
                    {
                        using (RepositoryItemMemoEdit edit = new RepositoryItemMemoEdit())
                        {
                            using (MemoEditViewInfo viewInfo = edit.CreateViewInfo() as MemoEditViewInfo)
                            {
                                viewInfo.EditValue = gridMain.GetRowCellValue(e.RowHandle, column.FieldName);
                                var junkstr = dt.Rows[thisRow][column.FieldName];
                                junkstr = viewInfo.EditValue.ObjToString();
                                length = junkstr.ObjToString().Length;
                                maxLength = Math.Max(length, maxLength);
                                viewInfo.EditValue = junkstr;
                                int cnt = junkstr.ObjToString().Count(c => c == '.');
                                cnt = junkstr.ObjToString().Count(c => c == '\n');
                                periods = Math.Max(cnt, periods);
                                //viewInfo.Bounds = new Rectangle(0, 0, column.VisibleWidth, dgv.Height);
                                //viewInfo.Bounds = new Rectangle(0, 0, column.VisibleWidth, gridMain.RowHeight);
                                viewInfo.Bounds = new Rectangle(0, 0, column.VisibleWidth, rowHeight);
                                using (Graphics graphics = dgv.CreateGraphics())
                                using (GraphicsCache cache = new GraphicsCache(graphics))
                                {
                                    viewInfo.CalcViewInfo(graphics);
                                    var height = ((IHeightAdaptable)viewInfo).CalcHeight(cache, column.VisibleWidth);
                                    //if (periods > 0)
                                    //    height += height * periods;
                                    newHeight = Math.Max(height, maxHeight);
                                    if (newHeight > maxHeight)
                                        maxHeight = newHeight;
                                    //repositoryItemMemoEdit1.AllowHtmlDraw = DefaultBoolean.False;
                                }
                            }
                        }
                    }
                }

                if (maxHeight > 0)
                {
                    if (maxHeight < 16)
                        maxHeight = 16;
                    else
                        e.RowHeight = maxHeight;
                }
                else
                    e.RowHeight = 16;
            }
        }
        /***********************************************************************************************/
        private void gridMain_CustomDrawCell(object sender, DevExpress.XtraGrid.Views.Base.RowCellCustomDrawEventArgs e)
        {
            if (e.Column.FieldName.ToUpper() == "DATA")
            {
                if (e.RowHandle >= 0)
                {
                    int row = gridMain.GetDataSourceRowIndex(e.RowHandle);
                    bool doit = false;
                    if (row == notesRow)
                        doit = true;
                    else if (row == resultsRow)
                        doit = true;
                    if ( doit )
                    {
                        DataTable dt = (DataTable)dgv.DataSource;
                        string field = dt.Rows[row]["field"].ObjToString().ToUpper();
                        if ( field == "NOTES" || field == "RESULTS")
                        {
                            //e.Cache.FillRectangle(Color.Salmon, e.Bounds);
                            e.Appearance.DrawString(e.Cache, e.DisplayText, e.Bounds);
                            e.Handled = true;
                        }
                    }
                }
            }
        }
        /***********************************************************************************************/
        private void CheckForEdit ( bool isTab = false )
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            if (isTab)
            {
                int rowHandle = gridMain.FocusedRowHandle;
                int row = gridMain.GetDataSourceRowIndex(rowHandle);
                DataTable ddt = (DataTable)dgv.DataSource;
                if ( (row+1) < ddt.Rows.Count )
                    dr = ddt.Rows[row + 1];
            }
            string field = dr["field"].ObjToString().ToUpper();
            if (field == "NOTES" || field == "RESULTS")
            {
                int rowhandle = gridMain.FocusedRowHandle;
                //rowhandle = hitInfo.RowHandle;
                int row = gridMain.GetDataSourceRowIndex(rowhandle);
                DataTable dt = (DataTable)dgv.DataSource;

                DateTime today = DateTime.Now;
                string data = dr["data"].ObjToString();
                if (!String.IsNullOrWhiteSpace(data))
                    data += "\n";
                data += today.ToString("MM/dd/yyyy") + " ";

                using (EditTextData fmrmyform = new EditTextData(field, data))
                {
                    fmrmyform.Text = "";
                    fmrmyform.ShowDialog();
                    if (fmrmyform.DialogResult == DialogResult.OK)
                    {
                        string p = fmrmyform.Answer.Trim();
                        dt.Rows[row]["data"] = p;
                        gridMain.RefreshEditor(true);
                        gridMain.RefreshData();
                    }
                }
            }
            else
            {
                int rowhandle = gridMain.FocusedRowHandle;
                //rowhandle = hitInfo.RowHandle;
                int row = gridMain.GetDataSourceRowIndex(rowhandle);
                DataTable dt = (DataTable)dgv.DataSource;

                DateTime today = DateTime.Now;
                string data = dr["data"].ObjToString();

                bool doDate = false;
                if (field.ToUpper().IndexOf("DATE") >= 0)
                    doDate = true;
                else if (field.ToUpper() == "DOB" )
                    doDate = true;

                if (doDate)
                {
                    DateTime myDate = data.ObjToDateTime();
                    using (GetDate dateForm = new GetDate(myDate, field, 0, true ))
                    {
                        dateForm.ShowDialog();
                        if (dateForm.DialogResult == System.Windows.Forms.DialogResult.OK)
                        {
                            try
                            {
                                myDate = dateForm.myDateAnswer;
                                DateTime date = myDate.ObjToDateTime();
                                dt.Rows[row]["data"] = date.ToString("MM/dd/yyyy");
                            }
                            catch (Exception ex)
                            {
                            }
                        }
                        else if (dateForm.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                        {
                            try
                            {
                                dt.Rows[row]["data"] = "";
                            }
                            catch (Exception ex)
                            {
                            }
                        }
                    }
                }
                else
                {
                    myDt.Rows.Clear();

                    isRepository(field, ref myDt);

                    if (myDt.Rows.Count > 0)
                    {
                        ciLookup.Items.Clear();
                        for (int i = 0; i < myDt.Rows.Count; i++)
                            ciLookup.Items.Add(myDt.Rows[i]["stuff"].ObjToString());
                        gridMain.Columns["data"].ColumnEdit = ciLookup;
                        //GridColumn currCol = gridMain.FocusedColumn;
                        //currCol.ColumnEdit = ciLookup;
                    }
                    else
                    {
                        gridMain.Columns["data"].ColumnEdit = null;
                        //GridColumn currCol = gridMain.FocusedColumn;
                        //currCol.ColumnEdit = null;
                    }
                }
                if (!isTab)
                {
                    gridMain.RefreshData();
                    gridMain.RefreshEditor(true);
                }
                return;
            }
        }
        /***********************************************************************************************/
        private void gridMain_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return )
            {
                try
                {
                    (sender as ColumnView).CloseEditor();
                    gridMain.PostEditor();
                    (sender as ColumnView).MoveNext();
                    CheckForEdit();
                }
                catch ( Exception ex)
                {
                }
            }
            else if (e.KeyCode == Keys.Tab )
            {
                try
                {
                    int rowHandle = gridMain.FocusedRowHandle;
                    int row = gridMain.GetDataSourceRowIndex(rowHandle);
                    //gridMain.FocusedRowHandle = row + 1;
                    //gridMain.SelectRow(row + 1);
                    //(sender as ColumnView).CloseEditor();
                    //gridMain.PostEditor();
                    //(sender as ColumnView).MoveNext();
                    CheckForEdit( true );
                }
                catch (Exception ex)
                {
                }
            }
            else if (e.KeyCode == Keys.Down )
            {
                try
                {
                    (sender as ColumnView).MoveNext();
                    CheckForEdit();
                    e.Handled = true;
                }
                catch ( Exception ex)
                {
                }
            }
            else if ( e.KeyCode == Keys.Up)
            {
                try
                {
                    (sender as ColumnView).MovePrev();
                    CheckForEdit();
                    e.Handled = true;
                }
                catch ( Exception ex)
                {
                }
            }
        }
        /***********************************************************************************************/
        private void gridMain_CellValueChanged(object sender, CellValueChangedEventArgs e)
        {
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            DataRow dr = gridMain.GetFocusedDataRow();
            DataTable dt = (DataTable)dgv.DataSource;
            if (dr == null)
                return;

            string field = dr["field"].ObjToString().ToUpper();
            string data = dr["data"].ObjToString();
            if (field.IndexOf("HOME PHONE") >= 0 || field.IndexOf("WORK PHONE") >= 0 || field.IndexOf("MOBILE PHONE") >= 0)
            {
                string phone = data;
                phone = AgentProspectReport.reformatPhone(phone, true);
                dr["data"] = phone;
            }
            else if (field.ToUpper() == "BIRTHDAY")
            {
                DateTime date = data.ObjToDateTime();
                int age = G1.CalculateAgeCorrect(date, DateTime.Now);
                DataRow[] dRows = dt.Select("field='age'");
                if (dRows.Length > 0)
                {
                    dRows[0]["data"] = age.ToString();
                }
            }
            else if (field.ToUpper() == "ZIP")
            {
                string zipCode = data.ObjToString();
                string city = "";
                string state = "";
                string county = "";

                // Get city, state, and county.
                string cmd = "Select * from `ref_states` where `state` = '" + state + "';";
                DataTable dx = G1.get_db_data(cmd);
                if (dx.Rows.Count > 0)
                    state = dx.Rows[0]["abbrev"].ObjToString();

                DataRow[] dRows = dt.Select("field='state'");
                if (dRows.Length > 0)
                {
                    dRows[0]["data"] = state.ToString();
                }
                /*
                bool rv = FunFamily.LookupZipcode(zipCode, ref city, ref state, ref county);
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
                        textEdit_patientCity.Text = city;
                    if (!String.IsNullOrWhiteSpace(state))
                        comboStates.Text = state;
                    if (!String.IsNullOrWhiteSpace(county))
                    {
                        ChangeVitalsField("deccounty", county);
                        txtCounty.Text = county;
                    }
                }
                */
            }
            btnAccept.Show();
            btnAccept.Refresh();
            btnCancel.Show();
            btnCancel.Refresh();

            //GridColumn currCol = gridMain.FocusedColumn;
            //string currentColumn = currCol.FieldName;
            //string what = dr[currentColumn].ObjToString();
        }
        /***********************************************************************************************/
        private void gridMain_ValidatingEditor(object sender, DevExpress.XtraEditors.Controls.BaseContainerValidateEditorEventArgs e)
        {
            GridView view = sender as GridView;
            DataRow dr = gridMain.GetFocusedDataRow();
            string field = dr["field"].ObjToString();
            string data = dr["data"].ObjToString();
            if (field == "Prospect Creation Date")
            {
                e.Valid = false;
                return;
            }
            else if ( field == "Funeral Home")
            {
                if (!ValidateFuneralHome(data))
                {
                    e.Valid = false;
                    return;
                }
            }
        }
        /****************************************************************************************/
        private bool ValidateFuneralHome(string data)
        {
            bool found = false;
            string funeralHome = "";
            for (int i = 0; i < ciLookup.Items.Count; i++)
            {
                funeralHome = ciLookup.Items[i].ObjToString().Trim();
                if (funeralHome == data)
                {
                    found = true;
                    break;
                }
            }
            return found;
        }
        private void gridMain_ShowingEditor(object sender, CancelEventArgs e)
        {
            int count = ciLookup.Items.Count;
            string what = textBox1.Text.Trim();
            if (String.IsNullOrWhiteSpace(what))
                return;
            textBox1_TextChanged(null, null);
        }

        private void gridMain_ShowFilterPopupCheckedListBox(object sender, DevExpress.XtraGrid.Views.Grid.FilterPopupCheckedListBoxEventArgs e)
        {

        }
        /***********************************************************************************************/
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            GridColumn currCol = gridMain.FocusedColumn;
            currentColumn = currCol.FieldName;

            int focusedRow = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(focusedRow);

            DataRow dr = gridMain.GetFocusedDataRow();
            string field = dr["field"].ObjToString();

            //gridMain.Columns["data"].ColumnEdit = null;

            if (currentColumn.ToUpper() != "DATA")
                return;

            //if (textBox1.Text.ToUpper() == field.ToUpper())
            //    return;

            comboBox1.Items.Clear();

            ciLookup.Items.Clear();
            if (myDt == null)
            {
                myDt = new DataTable();
                myDt.Columns.Add("stuff");
            }
            myDt.Rows.Clear();

            isRepository(field, ref myDt);

            if (myDt.Rows.Count > 0)
            {
                for (int i = 0; i < myDt.Rows.Count; i++)
                {
                    ciLookup.Items.Add(myDt.Rows[i]["stuff"].ObjToString());
                    comboBox1.Items.Add(myDt.Rows[i]["stuff"].ObjToString());
                }
                //currCol.ColumnEdit = ciLookup;
                gridMain.Columns["data"].ColumnEdit = ciLookup;
            }
            else
            {
                gridMain.Columns["data"].ColumnEdit = null;
                //currCol.ColumnEdit = null;
            }
        }
        /***********************************************************************************************/
    }
}