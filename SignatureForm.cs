using System.Drawing;
using System.Windows.Forms;
using GeneralLib;
using Microsoft.Ink;
/****************************************************************************************/
namespace SMFS
{
    /****************************************************************************************/
    public class SignatureForm : DevExpress.XtraEditors.XtraForm
    {
        private InkOverlay i_overlay;
        private System.Windows.Forms.PictureBox picSignature;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.ComponentModel.Container components = null;
        private Panel panelAll;
        private Panel panelBottom;
        private Panel panelTop;
        private string workTitle = "";
        private bool imageIn = false;
        /****************************************************************************************/
        public SignatureForm( string title, Image img)
        {
            InitializeComponent();
            workTitle = title;

            if (img != null)
            {
                // imgin = true;
                picSignature.Image = img;
                imageIn = true;
            }
        }
        /****************************************************************************************/
        public Image SignatureResult
        {
            get
            {
                if (i_overlay.Ink.Strokes.Count > 0)
                    return G1.ImageFromBytes(i_overlay.Ink.Save(PersistenceFormat.Gif));
                else if (picSignature.Image != null)
                    return picSignature.Image;
                else
                    return null;
            }
        }
        /****************************************************************************************/
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }
        /****************************************************************************************/
        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.picSignature = new System.Windows.Forms.PictureBox();
            this.btnClear = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.panelAll = new System.Windows.Forms.Panel();
            this.panelTop = new System.Windows.Forms.Panel();
            this.panelBottom = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.picSignature)).BeginInit();
            this.panelAll.SuspendLayout();
            this.panelTop.SuspendLayout();
            this.panelBottom.SuspendLayout();
            this.SuspendLayout();
            // 
            // picSignature
            // 
            this.picSignature.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.picSignature.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.picSignature.Dock = System.Windows.Forms.DockStyle.Fill;
            this.picSignature.Location = new System.Drawing.Point(0, 0);
            this.picSignature.Name = "picSignature";
            this.picSignature.Size = new System.Drawing.Size(481, 115);
            this.picSignature.TabIndex = 1;
            this.picSignature.TabStop = false;
            // 
            // btnClear
            // 
            this.btnClear.Location = new System.Drawing.Point(3, 15);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(146, 40);
            this.btnClear.TabIndex = 1;
            this.btnClear.Text = "Clear";
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // btnOK
            // 
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Location = new System.Drawing.Point(164, 15);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(145, 40);
            this.btnOK.TabIndex = 2;
            this.btnOK.Text = "OK";
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(325, 15);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(146, 40);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // panelAll
            // 
            this.panelAll.Controls.Add(this.panelBottom);
            this.panelAll.Controls.Add(this.panelTop);
            this.panelAll.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelAll.Location = new System.Drawing.Point(5, 5);
            this.panelAll.Name = "panelAll";
            this.panelAll.Size = new System.Drawing.Size(481, 184);
            this.panelAll.TabIndex = 4;
            // 
            // panelTop
            // 
            this.panelTop.Controls.Add(this.picSignature);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(481, 115);
            this.panelTop.TabIndex = 5;
            // 
            // panelBottom
            // 
            this.panelBottom.Controls.Add(this.btnClear);
            this.panelBottom.Controls.Add(this.btnOK);
            this.panelBottom.Controls.Add(this.btnCancel);
            this.panelBottom.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelBottom.Location = new System.Drawing.Point(0, 115);
            this.panelBottom.Name = "panelBottom";
            this.panelBottom.Size = new System.Drawing.Size(481, 69);
            this.panelBottom.TabIndex = 6;
            // 
            // SignatureForm
            // 
            this.Appearance.Options.UseFont = true;
            this.AutoScaleBaseSize = new System.Drawing.Size(7, 16);
            this.ClientSize = new System.Drawing.Size(491, 194);
            this.Controls.Add(this.panelAll);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "SignatureForm";
            this.Padding = new System.Windows.Forms.Padding(5);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Enter Digital Signature";
            this.Load += new System.EventHandler(this.SignatureForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.picSignature)).EndInit();
            this.panelAll.ResumeLayout(false);
            this.panelTop.ResumeLayout(false);
            this.panelBottom.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion
        /****************************************************************************************/
        private void SignatureForm_Load(object sender, System.EventArgs e)
        {
            this.Cursor = System.Windows.Forms.Cursors.WaitCursor;
            this.Text = workTitle;
            i_overlay = new InkOverlay(picSignature);
            i_overlay.Enabled = true;
            this.Cursor = System.Windows.Forms.Cursors.Default;
        }
        /****************************************************************************************/
        private void btnClear_Click(object sender, System.EventArgs e)
        {
            this.picSignature.Image = null;
            i_overlay.Ink.DeleteStrokes();
            if ( imageIn )
            {
                Image fdSig = new Bitmap(1, 1);
                picSignature.Image = fdSig;
            }
            picSignature.SizeMode = PictureBoxSizeMode.Normal;
            picSignature.Size = picSignature.Size;
            // bCleared = true;
        }
        /****************************************************************************************/
        private void btnOK_Click(object sender, System.EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
        /****************************************************************************************/
        private void btnCancel_Click(object sender, System.EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
        /****************************************************************************************/

        //		private void SignatureForm_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        //		{
        //			if(i_overlay.Ink.Dirty||(picSignature.Image == null && picin != null))
        //			{
        //				DialogResult dr = DevExpress.XtraEditors.XtraMessageBox.Show("The signature has changed, do you want to save changes?","Save Changes",MessageBoxButtons.YesNoCancel,MessageBoxIcon.Question);
        //				switch(dr)
        //				{
        //					case DialogResult.No:
        //						this.DialogResult = Cancel;
        //						break;
        //					case DialogResult.Cancel:
        //						e.Cancel = true;
        //						break;
        //				}
        //			}
        //		}
        /****************************************************************************************/
    }
}

