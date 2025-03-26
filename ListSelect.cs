using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using GeneralLib;
/*******************************************************************************************/
namespace SMFS
{
/*******************************************************************************************/
public class ListSelect : DevExpress.XtraEditors.XtraForm
	{
		// private string special_data      = "";
		private bool   closing           = false;
		private bool   work_multi        = true;
		private bool   maintain_order    = false;
		private string work_list         = "";
		private int    work_order        = 0;
		public static string list_detail = "";
        private Font work_font           = null;
		private string[] marked_list     = new string [20000];
		private string[] marked_order    = new string [20000];
/*******************************************************************************************/
		private System.Windows.Forms.ListBox listBox1;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.Button button2;
		private System.Windows.Forms.MenuStrip mainMenu1;
		private System.Windows.Forms.ToolStripMenuItem menuItem1;
		private System.Windows.Forms.ToolStripMenuItem menuItem2;
		private System.Windows.Forms.ToolStripMenuItem menuItem3;
		private System.Windows.Forms.ToolStripMenuItem menuItem4;
		private System.Windows.Forms.ToolStripMenuItem menuItem5;
        private Panel panelAll;
        private Panel panelBottom;
        private Panel panelTop;
		private System.ComponentModel.Container components = null;
/*******************************************************************************************/
		public ListSelect ( string list )
		{
			work_list      = list;
			work_multi     = true;
			maintain_order = false;
			InitializeComponent();
		}
/*******************************************************************************************/
		public ListSelect ( string list, bool multi )
		{
			work_list      = list;
			work_multi     = multi;
			maintain_order = false;
			InitializeComponent();
		}
/*******************************************************************************************/
        public ListSelect(string list, bool multi, Font font )
        {
            work_list      = list;
            work_multi     = multi;
            work_font      = font;
            maintain_order = false;
            InitializeComponent();
        }
/*******************************************************************************************/
		public ListSelect ( string list, bool multi, bool order )
		{
			work_list      = list;
			work_multi     = multi;
			maintain_order = order;
			InitializeComponent();
		}
/*******************************************************************************************/
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
/*******************************************************************************************/
		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.mainMenu1 = new System.Windows.Forms.MenuStrip();
            this.menuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItem3 = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItem4 = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItem5 = new System.Windows.Forms.ToolStripMenuItem();
            this.panelAll = new System.Windows.Forms.Panel();
            this.panelBottom = new System.Windows.Forms.Panel();
            this.panelTop = new System.Windows.Forms.Panel();
            this.mainMenu1.SuspendLayout();
            this.panelAll.SuspendLayout();
            this.panelBottom.SuspendLayout();
            this.panelTop.SuspendLayout();
            this.SuspendLayout();
            // 
            // listBox1
            // 
            this.listBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listBox1.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            this.listBox1.Location = new System.Drawing.Point(0, 0);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(521, 276);
            this.listBox1.TabIndex = 0;
            this.listBox1.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.listBox1_DrawItem);
            this.listBox1.SelectedIndexChanged += new System.EventHandler(this.listBox1_SelectedIndexChanged);
            this.listBox1.DoubleClick += new System.EventHandler(this.listBox1_DoubleClick);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(10, 10);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(48, 25);
            this.button1.TabIndex = 2;
            this.button1.Text = "Run";
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(142, 10);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(56, 25);
            this.button2.TabIndex = 3;
            this.button2.Text = "Select";
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // mainMenu1
            // 
            this.mainMenu1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItem1,
            this.menuItem3});
            this.mainMenu1.Location = new System.Drawing.Point(0, 0);
            this.mainMenu1.Name = "mainMenu1";
            this.mainMenu1.Size = new System.Drawing.Size(521, 24);
            this.mainMenu1.TabIndex = 4;
            // 
            // menuItem1
            // 
            this.menuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItem2});
            this.menuItem1.Name = "menuItem1";
            this.menuItem1.Size = new System.Drawing.Size(37, 20);
            this.menuItem1.Text = "File";
            // 
            // menuItem2
            // 
            this.menuItem2.Name = "menuItem2";
            this.menuItem2.Size = new System.Drawing.Size(93, 22);
            this.menuItem2.Text = "Exit";
            this.menuItem2.Click += new System.EventHandler(this.menuItem2_Click);
            // 
            // menuItem3
            // 
            this.menuItem3.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItem4,
            this.menuItem5});
            this.menuItem3.Name = "menuItem3";
            this.menuItem3.Size = new System.Drawing.Size(39, 20);
            this.menuItem3.Text = "Edit";
            // 
            // menuItem4
            // 
            this.menuItem4.Name = "menuItem4";
            this.menuItem4.Size = new System.Drawing.Size(136, 22);
            this.menuItem4.Text = "Select All";
            this.menuItem4.Click += new System.EventHandler(this.menuItem4_Click);
            // 
            // menuItem5
            // 
            this.menuItem5.Name = "menuItem5";
            this.menuItem5.Size = new System.Drawing.Size(136, 22);
            this.menuItem5.Text = "Unselect All";
            this.menuItem5.Click += new System.EventHandler(this.menuItem5_Click);
            // 
            // panelAll
            // 
            this.panelAll.Controls.Add(this.panelBottom);
            this.panelAll.Controls.Add(this.panelTop);
            this.panelAll.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelAll.Location = new System.Drawing.Point(0, 24);
            this.panelAll.Name = "panelAll";
            this.panelAll.Size = new System.Drawing.Size(521, 276);
            this.panelAll.TabIndex = 5;
            // 
            // panelBottom
            // 
            this.panelBottom.Controls.Add(this.button2);
            this.panelBottom.Controls.Add(this.button1);
            this.panelBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelBottom.Location = new System.Drawing.Point(0, 235);
            this.panelBottom.Name = "panelBottom";
            this.panelBottom.Size = new System.Drawing.Size(521, 41);
            this.panelBottom.TabIndex = 7;
            // 
            // panelTop
            // 
            this.panelTop.Controls.Add(this.listBox1);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(521, 276);
            this.panelTop.TabIndex = 6;
            // 
            // ListSelect
            // 
            this.Appearance.Options.UseFont = true;
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(521, 300);
            this.Controls.Add(this.panelAll);
            this.Controls.Add(this.mainMenu1);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MainMenuStrip = this.mainMenu1;
            this.Name = "ListSelect";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "ListSelect";
            this.Closing += new System.ComponentModel.CancelEventHandler(this.ListSelect_Closing);
            this.Load += new System.EventHandler(this.ListSelect_Load);
            this.ResizeEnd += new System.EventHandler(this.ListSelect_ResizeEnd);
            this.mainMenu1.ResumeLayout(false);
            this.mainMenu1.PerformLayout();
            this.panelAll.ResumeLayout(false);
            this.panelBottom.ResumeLayout(false);
            this.panelTop.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion
/*******************************************************************************************/
		private void ListSelect_Load(object sender, System.EventArgs e)
		{
			if ( !work_multi )
				this.button1.Hide();
			for ( int i=0; i<20000; i++ )
				marked_list[i] = "";
			this.listBox1.Items.Clear();
			string[] HeaderLines = work_list.Split('\n');
			int LineCount        = HeaderLines.Length;
			foreach ( string hstr in HeaderLines )
			{
				this.listBox1.Items.Add ( hstr );
			}
            if (work_font != null)
                this.listBox1.Font = work_font;
            this.BringToFront();
		}
/*******************************************************************************************/
		private void button1_Click(object sender, System.EventArgs e)
		{ // Exit Button Clicked
			OnListDone ();
		}
        /***************************************************************************************/
        public delegate void d_void_eventdone_string(string s);
        public event d_void_eventdone_string ListDone;
        protected void OnListDone()
        {
            list_detail = "";
            string data = build_data_list();
            if (!string.IsNullOrWhiteSpace(data))
                list_detail = data;
            if (ListDone != null)
            {
                if (!string.IsNullOrWhiteSpace(data))
                {
                    ListDone.Invoke(data);
                    if ( !closing)
                        this.Close();
                }
            }
            else
            {
				try
				{
					this.Close();
				}
				catch ( Exception ex)
				{
				}
            }
        }
/*******************************************************************************************/
		private string build_data_list ()
		{
			string data = "";
			if ( !maintain_order )
			{
				int rows    = this.listBox1.Items.Count;
				for ( int i=0; i<rows; i++ )
				{
					string text = marked_list[i];
					if ( text == "ON" )
					{
						if ( data.Length > 0 )
							data += "\n";
						string str  = this.listBox1.Items[i].ToString();
						data += str;
					}
				}
			}
			else
			{
				for ( int j=1; j<=work_order; j++ )
				{
					int rows    = this.listBox1.Items.Count;
					for ( int i=0; i<rows; i++ )
					{
						string text = marked_list[i];
						if ( text == "ON" )
						{
							string sorder = marked_order[i];
							if ( G1.validate_numeric ( sorder ) )
							{
								int order = G1.myint ( sorder );
								if ( order == j )
								{
									if ( data.Length > 0 )
										data += "\n";
									string str  = this.listBox1.Items[i].ToString();
									data += str;
								}
							}
						}
					}
				}
			}
			return data;
		}
/*******************************************************************************************/
		private void ListSelect_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (closing)
				return;
			closing    = true;
            OnListDone();
        }
        /*******************************************************************************************/
        private void HighlightSelection()
		{
			string str = (string) listBox1.SelectedItem;
			if ( str == null )
				return;
			int idx = listBox1.Items.IndexOf(str);
			string data = marked_list[idx];
			if ( data.Trim().ToUpper() == "ON" )
			{
				marked_list[idx] = "";
				marked_order[idx] = "";
			}
			else
			{
				work_order        = work_order + 1;
				marked_list[idx]  = "ON";
				marked_order[idx] = work_order.ToString();
                if ( !work_multi)
                {
                    if ( !string.IsNullOrWhiteSpace ( str))
                    {
                        list_detail = str;
                        //this.Close();
                    }
                }
			}
            this.Refresh();
		}
/*******************************************************************************************/
		private void listBox1_DoubleClick(object sender, System.EventArgs e)
		{
            //if (!work_multi)
            //    UnSelectAll();
			string str = (string) listBox1.SelectedItem;
			if ( str == null )
				return;
            if ( !work_multi )
            {
                this.Close();
                return;
            }
            int idx = listBox1.Items.IndexOf(str);
			string data = marked_list[idx];
			if ( data.Trim().ToUpper() == "ON" )
			{
				marked_list[idx] = "";
				marked_order[idx] = "";
			}
			else
			{
				work_order        = work_order + 1;
				marked_list[idx]  = "ON";
				marked_order[idx] = work_order.ToString();
			}
			if ( work_multi )
				this.Refresh();
			else
			{
				OnListDone();
                this.Close();
			}
		}
/*******************************************************************************************/
		private void button2_Click(object sender, System.EventArgs e)
		{ // Select Button Clicked
            if ( work_multi)
			    listBox1_DoubleClick ( null, null );
		}
/*******************************************************************************************/
		private void listBox1_DrawItem(object sender, System.Windows.Forms.DrawItemEventArgs e)
		{
			if ( closing )
				return;
			//
			// Draw the background of the ListBox control for each item.
			// Create a new Brush and initialize to a Black colored brush by default.
			//
			Rectangle rect;
			string cstr, str;
			e.DrawBackground();
			Brush myBrush = Brushes.Black;
			myBrush = new System.Drawing.SolidBrush(System.Drawing.SystemColors.WindowText);

			//			myBrush = System.Drawing.Brush;
			//			if ( this.listBox1.SelectedIndex == e.Index )
			//				myBrush = Brushes.White;
			int idx   = e.Index;
			bool doit = false;
			StringBuilder zstr=new StringBuilder(128), stratt=new StringBuilder(128);
			// bool selected = false;
			string item = "";
			if ( listBox1.SelectedItem != null )
				item = (string) listBox1.SelectedItem;
			if ( e.Index < 0 )
				return;
			str = ((ListBox)sender).Items[e.Index].ToString();
			cstr = marked_list[idx];
			if ( cstr == "ON" )
				doit = true;
			if ( doit == true )
			{
				rect = e.Bounds;
				myBrush = Brushes.Yellow;
				e.Graphics.FillRectangle ( myBrush, rect );
				myBrush = Brushes.Black;
				str = ((ListBox)sender).Items[e.Index].ToString();
				e.Graphics.DrawString( str,	e.Font, myBrush,e.Bounds,StringFormat.GenericDefault);
				e.DrawFocusRectangle();
				return;
			}
			str = ((ListBox)sender).Items[e.Index].ToString();
			doit = true;
			if ( str.Length > 4 && str[0] == '*' && str[1] == '*' && str[2] == '*' )
				doit = false;
			string measureString = "==================================================";
			measureString += "============================";
			string fontname = e.Font.Name;
			float fontpoint = e.Font.SizeInPoints;
			Font stringFont = new Font(fontname, fontpoint );
			// Measure string.
			SizeF stringSize = new SizeF();
			stringSize = e.Graphics.MeasureString(measureString, stringFont);
			float width = stringSize.Width / (float)78.0;
			if ( str.Length > 0 )
			{
				if ( str[0] == ' ' || str[0] == '=' || str[0] == '-' )
					doit = false;
			}
			if ( str.Length <= 35 )
				doit = false;
			if ( doit == false )
			{
				rect = e.Bounds;
				myBrush = Brushes.White;
				e.Graphics.FillRectangle ( myBrush, rect );
				myBrush = Brushes.Black;
				e.Graphics.DrawString(str,	e.Font, myBrush,e.Bounds,StringFormat.GenericDefault);
				e.DrawFocusRectangle();
				return;
			}
			if ( str.Length > 35 && doit == true )
			{
				if ( str[0] != ' ' && str[0] != '=' && str[0] != '-' )
				{
					cstr = str;
					cstr = str.Substring (0, 35);
					e.Graphics.DrawString(cstr,	e.Font, myBrush,e.Bounds,StringFormat.GenericDefault);
					int len = str.Length - 1;
					if ( len > 35 && len >= 47 )
					{
						cstr = str.Substring ( 35, 12 );
						myBrush = Brushes.Black;
						int at = cstr.IndexOf (" H ", 0, cstr.Length );
						if ( at > 0 )
							myBrush = Brushes.Red;
						at = cstr.IndexOf (" HH", 0, cstr.Length );
						if ( at > 0 )
							myBrush = Brushes.Red;
						at = cstr.IndexOf (" L ", 0, cstr.Length );
						if ( at > 0 )
							myBrush = Brushes.Blue;
						at = cstr.IndexOf (" LL", 0, cstr.Length );
						if ( at > 0 )
							myBrush = Brushes.Blue;
						at = cstr.IndexOf (" A ", 0, cstr.Length );
						if ( at > 0 )
							myBrush = Brushes.Red;
						at = cstr.IndexOf (" AA", 0, cstr.Length );
						if ( at > 0 )
							myBrush = Brushes.Red;
						float fx, fy;
						int x = e.Bounds.X;
						fx = (float) (x + (34.7*width));
						fy = (float) e.Bounds.Y;
						e.Graphics.DrawString(cstr,	e.Font, myBrush, fx, fy );
						if ( len > 47 )
						{
							cstr = str.Substring ( 47, len-47+1 );
							myBrush = Brushes.Black;
							fx = (float) (x + (46.7*width));
							e.Graphics.DrawString(cstr,	e.Font, myBrush, fx, fy );
						}
					}
				}
				else
				{
					cstr = str;
					e.Graphics.DrawString(cstr,	e.Font, myBrush,e.Bounds,StringFormat.GenericDefault);
				}
			}
			else
			{
				cstr = str;
				e.Graphics.DrawString(cstr,	e.Font, myBrush,e.Bounds,StringFormat.GenericDefault);
			}
			//
			// If the ListBox has focus, draw a focus rectangle around the selected item.
			//
			e.DrawFocusRectangle();
		}
/*******************************************************************************************/
		private void menuItem2_Click(object sender, System.EventArgs e)
		{
			this.Close ();
		}
/*******************************************************************************************/
		private void menuItem4_Click(object sender, System.EventArgs e)
		{ // Select All
			int last = this.listBox1.Items.Count;
			for ( int i=0; i<last; i++ )
				marked_list[i] = "ON";
			this.listBox1.Refresh();
			this.Refresh();
		}
/*******************************************************************************************/
        private void UnSelectAll()
        {
			int last = this.listBox1.Items.Count;
			for ( int i=0; i<last; i++ )
				marked_list[i] = "";
			this.listBox1.Refresh();
			this.Refresh();
        }
/*******************************************************************************************/
		private void menuItem5_Click(object sender, System.EventArgs e)
		{ // UnSelect All
            UnSelectAll();
		}
/*******************************************************************************************/
        private void ListSelect_ResizeEnd(object sender, System.EventArgs e)
        {
            listBox1.Refresh();
            this.Refresh();
        }
/*******************************************************************************************/
        private void listBox1_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            if (!work_multi)
            {
                UnSelectAll();
                HighlightSelection();
            }
        }
/*******************************************************************************************/
	}
}
