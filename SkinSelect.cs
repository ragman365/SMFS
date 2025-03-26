using System;
using System.Data;
using System.Drawing;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using System.Reflection;
//using EMRClassLib.Preferences;
/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public partial class SkinSelect : DevExpress.XtraEditors.XtraForm
    {
        private DataTable SkinDt;
        private DataTable ColorDt;
        public static string SkinSelect_Skin = "";
        public static string SkinSelect_Color = "";
        public static bool SkinSelect_NoColor = false;
/***********************************************************************************************/
        public SkinSelect( string ActiveSkin )
        {
            InitializeComponent();
        }
/***********************************************************************************************/
        private void SkinSelect_Load(object sender, EventArgs e)
        {
            PopulateMySkinsMenu();
            radioSkin_CheckedChanged(null, null);
            SkinSelect_Skin = "";
            SkinSelect_Color = "";
            SkinSelect_NoColor = false;
        }
/***********************************************************************************************/
        private void LoadSkinToGrid(string skin)
        {
            DataRow dRow = SkinDt.NewRow();
            dRow["Skin"]= skin;
            SkinDt.Rows.Add(dRow);
            SkinDt.AcceptChanges();
        }
/***********************************************************************************************/
        private void LoadSkins()
        {
            if (SkinDt == null)
            {
                SkinDt = new DataTable();
                SkinDt.Columns.Add("Skin");
                skinView.RowCellClick += skinView_RowCellClick;

                ColorDt = new DataTable();
                ColorDt.Columns.Add("Even");
                ColorDt.Columns.Add("Color");
                string[] colors = Enum.GetNames(typeof(KnownColor));
                for (int i = 0; i < colors.Length; i++)
                {
                    string c = colors[i];
                    DataRow dRow = ColorDt.NewRow();
                    dRow["Color"] = c;
                    ColorDt.Rows.Add(dRow);
                }
                ColorDt.AcceptChanges();
            }
        }
/***********************************************************************************************/
        void skinView_RowCellClick(object sender, RowCellClickEventArgs e)
        {
            int rowHandle = -1;
            DevExpress.XtraGrid.Columns.GridColumn column = null;
            string ColumnName = "";
            string fieldName = "";
            DevExpress.XtraGrid.Views.Grid.GridView view = (DevExpress.XtraGrid.Views.Grid.GridView)(dgv2.MainView);
            GridHitInfo hitInfo = view.CalcHitInfo(e.Location);
            if (hitInfo.InRowCell)
            {
                rowHandle = hitInfo.RowHandle;
                column = hitInfo.Column;
                ColumnName = column.Caption.Trim().ToUpper();
                fieldName = column.FieldName.Trim().ToUpper();
            }
            DataRow dr = view.GetDataRow(rowHandle);
            if (dr == null)
                return;
            string skin = "";
            string color = "";
            if (ColumnName.Trim().ToUpper() == "SKIN")
            {
                skin = dr["Skin"].ToString();
                if (skin.Trim().Length == 0)
                    skin = "Windows Default";
                this.LookAndFeel.SetSkinStyle(skin);
                OnSkinSelect("Skin : " + skin);
                SkinSelect_Skin = skin;
            }
            else if (ColumnName.Trim().ToUpper() == "EVEN")
            {
                color = dr["color"].ToString();
                OnSkinSelect("Color : " + color);
                SkinSelect_Color = color;
                colorChanging = true;
                this.chkNoColor.Checked = false;
                colorChanging = false;
            }
            else if (ColumnName.Trim().ToUpper() == "COLOR")
            {
                color = dr["color"].ToString();
                OnSkinSelect("Color : " + color);
                SkinSelect_Color = color;
                colorChanging = true;
                this.chkNoColor.Checked = false;
                colorChanging = false;
            }
        }
/*******************************************************************************************/
        static DevExpress.XtraBars.BarButtonItem ActiveSkin;
        private void PopulateMySkinsMenu()
        {
            LoadSkins();
            PopulateSkinsMenu ();
        }
/*******************************************************************************************/
        private void PopulateSkinsMenu()
        {
            DevExpress.Skins.SkinContainerCollection skins = DevExpress.Skins.SkinManager.Default.Skins;

            LoadSkinToGrid("Windows Default");
            DevExpress.UserSkins.BonusSkins.Register();

            foreach (DevExpress.Skins.SkinContainer s in skins)
            {
                PropertyInfo pi = s.GetType().GetProperty("Creator", BindingFlags.NonPublic | BindingFlags.Instance);
                DevExpress.Skins.Info.SkinXmlCreator creator = pi.GetValue(s, null) as DevExpress.Skins.Info.SkinXmlCreator;
                pi = creator.GetType().GetProperty("SkinAssembly", BindingFlags.NonPublic | BindingFlags.Instance);
                Assembly assembly = pi.GetValue(creator, null) as Assembly;
                AssemblyDescriptionAttribute atr = AssemblyDescriptionAttribute.GetCustomAttribute(assembly, typeof(AssemblyDescriptionAttribute)) as AssemblyDescriptionAttribute;
                string category = (atr == null) ? "" : atr.Description;

                LoadSkinToGrid(s.SkinName);
            }
        }
/***********************************************************************************************/
        private void radioColor_CheckedChanged(object sender, System.EventArgs e)
        {
            skinView.Columns["skin"].Visible = false;
            skinView.Columns["even"].Visible = true;
            skinView.Columns["color"].Visible = true;
            dgv2.DataSource = ColorDt;
            dgv2.Refresh();
        }
/***********************************************************************************************/
        private void radioSkin_CheckedChanged(object sender, System.EventArgs e)
        {
            skinView.Columns["skin"].Visible = true;
            skinView.Columns["even"].Visible = false;
            skinView.Columns["color"].Visible = false;
            dgv2.DataSource = SkinDt;
            dgv2.Refresh();
        }
/***********************************************************************************************/
        private bool colorChanging = false;
        private void chkNoColor_CheckedChanged(object sender, EventArgs e)
        {
            if (colorChanging)
                return;
            DevExpress.XtraGrid.Views.Grid.GridView view = (DevExpress.XtraGrid.Views.Grid.GridView)(dgv2.MainView);
            if (chkNoColor.Checked)
            {
                OnSkinSelect("No Color On");
                SkinSelect_NoColor = true;
                view.Appearance.EvenRow.Options.UseBackColor = false;
                view.Appearance.OddRow.Options.UseBackColor = false;
            }
            else
            {
                OnSkinSelect("No Color Off");
                SkinSelect_NoColor = false;
                view.Appearance.EvenRow.Options.UseBackColor = true;
                view.Appearance.OddRow.Options.UseBackColor = true;
            }
        }
/***********************************************************************************************/
        private void skinView_CustomDrawCell(object sender, RowCellCustomDrawEventArgs e)
        {
            if (e.Column.FieldName.ToString().ToUpper() == "SKIN")
            {
                GridView view = sender as GridView;
                if (e.RowHandle >= 0)
                {
                    e.DisplayText = SkinDt.Rows[e.RowHandle]["Skin"].ToString();
                }
            }
            else if (e.Column.FieldName.ToString().ToUpper() == "COLOR")
            {
                GridView view = sender as GridView;
                if (e.RowHandle >= 0)
                {
                    e.DisplayText = ColorDt.Rows[e.RowHandle]["Color"].ToString();
                }
            }
            else if (e.Column.FieldName.ToString().ToUpper() == "EVEN")
            {
                GridView view = sender as GridView;
                if (e.RowHandle >= 0)
                {
                    string color = ColorDt.Rows[e.RowHandle]["Color"].ToString();
                    Graphics g = e.Graphics;
                    g.FillRectangle(new SolidBrush(Color.FromName(color)), e.Bounds);
                }
            }
        }
        /***************************************************************************************/
        public delegate void d_void_eventdone_string(string s);
        public event d_void_eventdone_string SkinSelected;
        protected void OnSkinSelect( string message )
        {
            if (SkinSelected != null)
            {
                this.BeginInvoke(SkinSelected, new object[] { message });
            }
        }
/***********************************************************************************************/
    }
}