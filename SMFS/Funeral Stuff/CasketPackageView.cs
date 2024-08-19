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
using System.Threading;
using GeneralLib;
using System.IO;
/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public partial class CasketPackageView : DevExpress.XtraEditors.XtraForm
    {
        private string workCasketGroup = "";
        private DataTable workDt = null;
        private DataTable originalDt = null;
        private int position = 0;
        private int lastPosition = 0;
        private Bitmap emptyImage;
        private Label label = null;
        private Label casketLabel = null;
        private Label packageLabel = null;

        private int screenWidth = 0;
        private int screenHeight = 0;
        private double widthRatio = 5;
        private double heightRatio = 4;

        private string sortField = "";

        /***********************************************************************************************/
        public CasketPackageView( string casketGroup, DataTable dt )
        {
            InitializeComponent();
            workCasketGroup = casketGroup;
            originalDt = dt;
            //RemoveEmpties();
            workDt = originalDt;
            LoadComboType();
        }
        /***********************************************************************************************/
        private void CasketPackageView_Load(object sender, EventArgs e)
        {

            SetupScreen();
            position = 0;
            lastPosition = workDt.Rows.Count;
            emptyImage = new Bitmap(1, 1);
            label1.Hide();
            label2.Hide();
            txtBasicService.Hide();
            txtCasketCost.Hide();
            LoadData();
            this.Cursor = Cursors.Arrow;
        }
        /***********************************************************************************************/
        private void RemoveEmpties()
        {
            for (int i = originalDt.Rows.Count - 1; i >= 0; i--)
            {
                if (originalDt.Rows[i]["picture1"] == null)
                    originalDt.Rows.RemoveAt(i);
            }
        }
        /***********************************************************************************************/
        private void SetupScreen()
        {
            Rectangle rect = GetScreen();
            int width = rect.Right - rect.Left;
            int height = rect.Bottom - rect.Top;
            height = System.Windows.Forms.SystemInformation.PrimaryMonitorSize.Height;
            screenHeight = height;
            width = System.Windows.Forms.SystemInformation.PrimaryMonitorSize.Width;
            width = this.Width - 60;
            this.Width = width;
            screenWidth = width;
            this.Height = (int)(heightRatio * width / widthRatio);
//            this.Width = width;
            //if ( width > height)
            //{
            //    this.Height = height - 42;
            //    this.Width = height;
            //}
            //else
            //{
            //    this.Height = width;
            //    this.Width = width;
            //}
            int left = rect.Right - this.Width;
            int top = this.Top;
            this.SetBounds(left, top, this.Width, this.Height);
            label = new Label();
            FontFamily fontFamily = new FontFamily("Monotype Corsiva");
            Font font = new Font( fontFamily, 20, FontStyle.Bold, GraphicsUnit.Pixel);
            label.Font = font;
            label.RightToLeft = RightToLeft.No;
            label.Anchor = AnchorStyles.Right;
            label.Dock = DockStyle.Right;
            label.BackColor = Color.Transparent;
            label.Left = pictureBox1.Right - 201;
            label.Top = 10;
            label.MaximumSize = new Size(200, 0);
            label.AutoSize = true;
            label.Text = "";
            pictureBox1.Controls.Add(label);

            casketLabel = new Label();
            fontFamily = new FontFamily("Tahoma");
            font = new Font( fontFamily, 16, FontStyle.Regular, GraphicsUnit.Pixel);
            casketLabel.Font = font;
            casketLabel.RightToLeft = RightToLeft.No;
            casketLabel.Dock = DockStyle.Bottom;
            casketLabel.BackColor = Color.Transparent;
            casketLabel.Left = 10;
            casketLabel.Width = 500;
            casketLabel.MaximumSize = new Size(500, casketLabel.Height);

            casketLabel.Top = pictureBox1.Bottom - 60;
            casketLabel.Text = "";
            pictureBox1.Controls.Add(casketLabel);

            packageLabel = new Label();
            fontFamily = new FontFamily("Tahoma");
            font = new Font(fontFamily, 16, FontStyle.Regular, GraphicsUnit.Pixel);
            packageLabel.Font = font;
            packageLabel.RightToLeft = RightToLeft.No;
            packageLabel.Dock = DockStyle.Bottom;
            packageLabel.BackColor = Color.Transparent;
            packageLabel.Left = 10;
            packageLabel.Width = 500;
            packageLabel.MaximumSize = new Size(500, packageLabel.Height);

            packageLabel.Top = pictureBox1.Bottom - 40;
            packageLabel.Text = "";
            pictureBox1.Controls.Add(packageLabel);
//            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        public Rectangle GetScreen()
        {
            return Screen.FromControl(this).Bounds;
        }
        /***********************************************************************************************/
        private void LoadVaults()
        {
            casketLabel.Text = "";
            string casketCode = workDt.Rows[position]["casketcode"].ObjToString();
            string casketDesc = workDt.Rows[position]["casketdesc"].ObjToString();
            double casketCost = workDt.Rows[position]["casketcost"].ObjToDouble();
            casketCost = G1.RoundDown(casketCost);
            string str = G1.ReformatMoney(casketCost);
            int idx = str.IndexOf('.');
            if (idx > 0)
                str = str.Substring(0, idx);
            this.txtCasketCost.Text = "$" + str;
            this.Text = casketDesc;
            string cmd = "Select * from `inventorylist` where `casketcode` = '" + casketCode + "';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count > 0)
            {
                Byte[] bytes = dx.Rows[0]["picture"].ObjToBytes();
                Image myImage = emptyImage;
                if (bytes != null)
                {
                    myImage = G1.byteArrayToImage(bytes);
                    this.pictureBox1.Image = (Bitmap)myImage;
                    label.Text = casketDesc;
                    casketLabel.Text = "Vault: " + this.txtCasketCost.Text;
                    packageLabel.Text = "";
                }
            }
            lblWhich.Text = (position + 1).ToString() + " of " + lastPosition.ToString();
            lblWhich.Refresh();
        }
        /***********************************************************************************************/
        private void LoadData ()
        {
            if ( workCasketGroup.ToUpper() == "MASTER VAULT")
            {
                LoadVaults();
                return;
            }
            int maxRow = workDt.Rows.Count - 1;
            if (position > maxRow)
                position = maxRow;
            if (position < 0)
                position = 0;
            string masterRecord = workDt.Rows[position]["!masterRecord"].ObjToString();
            double casketCost = workDt.Rows[position]["casket"].ObjToDouble();
            double packageCost = workDt.Rows[position]["package"].ObjToDouble();
            casketCost = G1.RoundDown(casketCost);
            packageCost = G1.RoundDown(packageCost);
            string str = G1.ReformatMoney(casketCost);
            int idx = str.IndexOf('.');
            if (idx > 0)
                str = str.Substring(0, idx);
            this.txtCasketCost.Text = "$" + str;
            str = G1.ReformatMoney(packageCost);
            idx = str.IndexOf('.');
            if (idx > 0)
                str = str.Substring(0, idx);
            this.txtBasicService.Text = "$" + str;
            this.pictureBox1.Image = emptyImage;
            label.Text = "";
            casketLabel.Text = "";
            packageLabel.Text = "";
            string cmd = "Select * from `casket_master` where `record` = '" + masterRecord + "';";
            DataTable dx = G1.get_db_data(cmd);
            if ( dx.Rows.Count > 0 )
            {
                string casketCode = dx.Rows[0]["casketcode"].ObjToString();
                string casketDesc = dx.Rows[0]["casketdesc"].ObjToString();
                this.Text = casketDesc;
                casketDesc = casketDesc.Replace(casketCode, "").Trim();
                cmd = "Select * from `inventorylist` where `casketcode` = '" + casketCode + "';";
                dx = G1.get_db_data(cmd);
                if ( dx.Rows.Count > 0 )
                {
                    Byte[] bytes = dx.Rows[0]["picture"].ObjToBytes();
                    Image myImage = emptyImage;
                    if (bytes != null)
                    {
                        myImage = G1.byteArrayToImage(bytes);
                        this.pictureBox1.Image = (Bitmap)myImage;
                        label.Text = casketDesc;
                        casketLabel.Text = "Casket: " + this.txtCasketCost.Text;
                        packageLabel.Text = "Casket with Full Traditional Service: " + this.txtBasicService.Text;
                    }
                }
            }
            lblWhich.Text = (position+1).ToString() + " of " + lastPosition.ToString();
            lblWhich.Refresh();
        }
        /***********************************************************************************************/
        private void LoadComboType ()
        {
            DataTable dt = workDt.Copy();
            sortField = "caskettype1";
            if ( G1.get_column_number ( dt, "caskettype1") < 0 )
            {
                if (G1.get_column_number(dt, "caskettype") < 0)
                    return;
                sortField = "caskettype";
            }
            DataView tempview = dt.DefaultView;
            tempview.Sort = sortField;
            dt = tempview.ToTable();
            string oldType = "";
            string type = "";
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                type = dt.Rows[i][sortField].ObjToString();
                if ( type != oldType )
                    cmbType.Items.Add(type);
                oldType = type;
            }
        }
        /***********************************************************************************************/
        private void btnRight_Click(object sender, EventArgs e)
        {
            if ( (position+1) >= lastPosition )
            {
                position = -1;
            }
            position++;
            LoadData();
        }
        /***********************************************************************************************/
        private void btnLeft_Click(object sender, EventArgs e)
        {
            if ((position - 1) < 0 )
            {
                position = lastPosition + 1;
            }
            position--;
            LoadData();
        }
        /***************************************************************************************/
        public delegate void d_void_eventdone( int position, DataTable casketDt );
        public event d_void_eventdone SelectDone;
        protected void OnSelectDone()
        {
            SelectDone?.Invoke( position, workDt );
        }
        /***************************************************************************************/
        public delegate void d_void_eventdoneanyway();
        public event d_void_eventdoneanyway SelectDoneAnyway;
        protected void OnSelectDoneAnyway()
        {
            SelectDoneAnyway?.Invoke();
        }
        /***********************************************************************************************/
        private void btnSelect_Click(object sender, EventArgs e)
        {
            if (SelectDone != null)
            {
                OnSelectDone();
                this.Close();
            }
        }

        private void CasketPackageView_FormClosing(object sender, FormClosingEventArgs e)
        {
            if ( SelectDoneAnyway != null)
                OnSelectDoneAnyway();
        }
        /***********************************************************************************************/
        private void CasketPackageView_Resize(object sender, EventArgs e)
        {
            this.Height = (int)(heightRatio * this.Width / widthRatio);
        }
        /***********************************************************************************************/
        private void cmbType_SelectedIndexChanged(object sender, EventArgs e)
        {
            string type = cmbType.Text.Trim();
            DataRow[] dRows = originalDt.Select(sortField + "='" + type + "'");
            workDt = originalDt.Clone();
            for (int i = 0; i < dRows.Length; i++)
                workDt.ImportRow(dRows[i]);

            DataView tempview = workDt.DefaultView;
            tempview.Sort = sortField + " asc, casket desc";
            if ( workCasketGroup.ToUpper() == "MASTER VAULT")
                tempview.Sort = sortField + " asc, casketcost desc";

            workDt = tempview.ToTable();

            lastPosition = workDt.Rows.Count;
            position = 0;
            LoadData();
        }
        /***********************************************************************************************/
    }
}