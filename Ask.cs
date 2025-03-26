using System;
using System.Media;
using System.Windows.Forms;

namespace GeneralLib
{
/****************************************************************************************/
    public class Ask : DevExpress.XtraEditors.XtraForm
	{
		private System.Windows.Forms.TextBox txtAnswer;
		private System.Windows.Forms.Label lblQuestion;
        private DevExpress.XtraEditors.SimpleButton btnAccept;
        private DevExpress.XtraEditors.SimpleButton btnCancel;
		private System.ComponentModel.Container components = null;
		private string _answer = "";
        private DevExpress.XtraEditors.PanelControl panelControl1;
        private int _Restriction = 0;

        public string Answer { get { return _answer; } }
/****************************************************************************************/
        /// <summary>
        /// Ask a question
        /// </summary>
        /// <param name="question"></param>
		public Ask( string question )
		{
			InitializeComponent();
			if ( question.IndexOf ( "?" ) < 0 )
				question    += "?";
			this.lblQuestion.Text = question;
		}

        /// <summary>
        /// Ask a question
        /// </summary>
        /// <param name="question">Question to ask</param>
        /// <param name="restrictlength">Length restriction of answer textbox</param>
        public Ask(string question, int restrictlength)
            : this(question)
        {
            _Restriction = restrictlength;
        }
/****************************************************************************************/
        /// <summary>
        /// Ask a question
        /// </summary>
        /// <param name="question">Question to ask</param>
        /// <param name="answer"></param>
        public Ask(string question, string answer)
            : this(question)
        {
            this.txtAnswer.Text = answer;
        }
/****************************************************************************************/
        public Ask(string question, bool dictate)
            : this(question)
        {
        }
/****************************************************************************************/
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}
/****************************************************************************************/
		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.txtAnswer = new System.Windows.Forms.TextBox();
            this.lblQuestion = new System.Windows.Forms.Label();
            this.btnAccept = new DevExpress.XtraEditors.SimpleButton();
            this.btnCancel = new DevExpress.XtraEditors.SimpleButton();
            this.panelControl1 = new DevExpress.XtraEditors.PanelControl();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl1)).BeginInit();
            this.panelControl1.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtAnswer
            // 
            this.txtAnswer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtAnswer.Location = new System.Drawing.Point(5, 50);
            this.txtAnswer.Name = "txtAnswer";
            this.txtAnswer.Size = new System.Drawing.Size(237, 21);
            this.txtAnswer.TabIndex = 0;
            this.txtAnswer.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtAnswer_KeyPress);
            this.txtAnswer.KeyUp += new System.Windows.Forms.KeyEventHandler(this.txtAnswer_KeyUp);
            // 
            // lblQuestion
            // 
            this.lblQuestion.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblQuestion.Location = new System.Drawing.Point(5, 2);
            this.lblQuestion.MinimumSize = new System.Drawing.Size(240, 25);
            this.lblQuestion.Name = "lblQuestion";
            this.lblQuestion.Size = new System.Drawing.Size(240, 45);
            this.lblQuestion.TabIndex = 1;
            // 
            // btnAccept
            // 
            this.btnAccept.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAccept.Appearance.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAccept.Appearance.Options.UseFont = true;
            this.btnAccept.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnAccept.Location = new System.Drawing.Point(5, 77);
            this.btnAccept.Name = "btnAccept";
            this.btnAccept.Size = new System.Drawing.Size(75, 25);
            this.btnAccept.TabIndex = 2;
            this.btnAccept.Text = "Accept";
            this.btnAccept.Click += new System.EventHandler(this.btnAccept_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.Appearance.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCancel.Appearance.Options.UseFont = true;
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(160, 77);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 25);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // panelControl1
            // 
            this.panelControl1.Controls.Add(this.btnAccept);
            this.panelControl1.Controls.Add(this.txtAnswer);
            this.panelControl1.Controls.Add(this.lblQuestion);
            this.panelControl1.Controls.Add(this.btnCancel);
            this.panelControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelControl1.Location = new System.Drawing.Point(0, 0);
            this.panelControl1.Name = "panelControl1";
            this.panelControl1.Size = new System.Drawing.Size(247, 107);
            this.panelControl1.TabIndex = 5;
            // 
            // Ask
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 14);
            this.ClientSize = new System.Drawing.Size(247, 107);
            this.Controls.Add(this.panelControl1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "Ask";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Load += new System.EventHandler(this.Ask_Load);
            ((System.ComponentModel.ISupportInitialize)(this.panelControl1)).EndInit();
            this.panelControl1.ResumeLayout(false);
            this.panelControl1.PerformLayout();
            this.ResumeLayout(false);

		}
		#endregion
/****************************************************************************************/
		private void Ask_Load(object sender, System.EventArgs e)
		{
            this.TopMost = true;
            //calculate_width ();
			this.Focus ();
		}
/****************************************************************************************/

		private void btnAccept_Click(object sender, System.EventArgs e)
		{
			_answer   = this.txtAnswer.Text;
		}
/****************************************************************************************/
		private void btnCancel_Click(object sender, System.EventArgs e)
		{
			_answer   = "";
		}
/****************************************************************************************/
		private void txtAnswer_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
		{
            if (e.KeyCode == Keys.Enter && txtAnswer.Text.Length > 0)
            {
                btnAccept_Click(this, new EventArgs());
                this.DialogResult = DialogResult.OK;
            }
		}
/****************************************************************************************/
        private void txtAnswer_KeyPress(object sender, KeyPressEventArgs e)
        {
            if((_Restriction > 0) && ((TextBox)sender).Text.Length >= _Restriction)
            {
                e.Handled = true;
                SystemSounds.Beep.Play();
            }
        }
/****************************************************************************************/
	}
}
