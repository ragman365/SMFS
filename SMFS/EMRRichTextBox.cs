using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using System.Threading;
using System.Text;
using System.Runtime.InteropServices;
using System.Drawing.Printing;
using System.IO;

namespace EMRControlLib
{
	#region StampActions
	public enum StampActions
	{
		EditedBy = 1,
		DateTime = 2,
		Custom = 4
	}
	#endregion

	/// <summary>
	/// An extended RichTextBox that contains a toolbar.
	/// </summary>
	public class EMRRichTextBox : System.Windows.Forms.UserControl
	{
		public enum ButtonType{Open,Save,Text,TextSize,Bold,Italic,Underline,Strike,Left,Center,Right,Undo,Redo,Stamp,
			Color,Spell};
		private System.Windows.Forms.ToolBar tb1;
		#region Windows Generated

		private System.Windows.Forms.ImageList imgList1;
		private System.Windows.Forms.ToolBarButton tbbBold;
		private System.Windows.Forms.ToolBarButton tbbItalic;
		private System.Windows.Forms.ToolBarButton tbbUnderline;
		private System.Windows.Forms.ToolBarButton tbbCenter;
		private System.Windows.Forms.ToolBarButton tbbRight;
		private System.Windows.Forms.ToolBarButton tbbStrikeout;
		private System.Windows.Forms.ToolBarButton tbbColor;
		private System.Windows.Forms.ContextMenu cmColors;
		private System.Windows.Forms.MenuItem miBlack;
		private System.Windows.Forms.MenuItem miBlue;
		private System.Windows.Forms.MenuItem miRed;
		private System.Windows.Forms.MenuItem miGreen;
		private System.Windows.Forms.ToolBarButton tbbStamp;
		private System.Windows.Forms.ToolBarButton tbbOpen;
		private System.Windows.Forms.ToolBarButton tbbSave;
		private System.Windows.Forms.ToolBarButton tbbUndo;
		private System.Windows.Forms.ToolBarButton tbbRedo;
		private System.Windows.Forms.ToolBarButton tbbSeparator2;
		private System.Windows.Forms.ToolBarButton tbbSeparator3;
		private System.Windows.Forms.ToolBarButton tbbSeparator4;
		private System.Windows.Forms.ToolBarButton tbbSeparator1;
		private System.Windows.Forms.ToolBarButton tbbLeft;
		private System.Windows.Forms.OpenFileDialog ofd1;
		private System.Windows.Forms.SaveFileDialog sfd1;
		private System.Windows.Forms.ContextMenu cmFonts;
		private System.Windows.Forms.MenuItem miArial;
		private System.Windows.Forms.MenuItem miGaramond;
		private System.Windows.Forms.MenuItem miLucidaConsole;
		private System.Windows.Forms.MenuItem miTahoma;
		private System.Windows.Forms.MenuItem miTimesNewRoman;
		private System.Windows.Forms.MenuItem miVerdana;
		private System.Windows.Forms.ToolBarButton tbbFont;
		private System.Windows.Forms.ToolBarButton tbbFontSize;
		private System.Windows.Forms.ToolBarButton tbbSeparator5;
		private System.Windows.Forms.MenuItem miCourierNew;
		private System.Windows.Forms.MenuItem miMicrosoftSansSerif;
		private System.Windows.Forms.ContextMenu cmFontSizes;
		private System.Windows.Forms.MenuItem mi7;
		private System.Windows.Forms.MenuItem mi8;
		private System.Windows.Forms.MenuItem mi9;
		private System.Windows.Forms.MenuItem mi10;
		private System.Windows.Forms.MenuItem mi11;
		private System.Windows.Forms.MenuItem mi12;
        private System.Windows.Forms.MenuItem mi13;
        private System.Windows.Forms.MenuItem mi14;
		private System.Windows.Forms.MenuItem mi16;
		private System.Windows.Forms.MenuItem mi18;
		private System.Windows.Forms.MenuItem mi20;
		private System.Windows.Forms.MenuItem mi22;
		private System.Windows.Forms.MenuItem mi24;
		private System.Windows.Forms.MenuItem mi26;
		private System.Windows.Forms.MenuItem mi28;
		private System.Windows.Forms.MenuItem mi36;
		private System.Windows.Forms.MenuItem mi48;
		private System.Windows.Forms.MenuItem mi72;
		private System.Windows.Forms.ToolBarButton tbbSparator6;
		private System.Windows.Forms.ToolBarButton tbbSpell;
		private System.ComponentModel.IContainer components;
		private NetSpell.SpellChecker.Spelling spelling;
        private EMRControlLib.RichTextBoxEx rtb1;
		private NetSpell.SpellChecker.Dictionary.WordDictionary wordDictionary;

		public EMRRichTextBox()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			//Update the graphics on the toolbar
			UpdateToolbar();
		}

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				rtb1.Dispose();
				rtb1 = null;
				spelling.Dispose();
				spelling = null;
				if(components != null)
					components.Dispose();
			}
			base.Dispose( disposing );
		}
		#endregion

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EMRRichTextBox));
            this.tb1 = new System.Windows.Forms.ToolBar();
            this.tbbSave = new System.Windows.Forms.ToolBarButton();
            this.tbbOpen = new System.Windows.Forms.ToolBarButton();
            this.tbbSeparator3 = new System.Windows.Forms.ToolBarButton();
            this.tbbFont = new System.Windows.Forms.ToolBarButton();
            this.cmFonts = new System.Windows.Forms.ContextMenu();
            this.miArial = new System.Windows.Forms.MenuItem();
            this.miCourierNew = new System.Windows.Forms.MenuItem();
            this.miGaramond = new System.Windows.Forms.MenuItem();
            this.miLucidaConsole = new System.Windows.Forms.MenuItem();
            this.miMicrosoftSansSerif = new System.Windows.Forms.MenuItem();
            this.miTahoma = new System.Windows.Forms.MenuItem();
            this.miTimesNewRoman = new System.Windows.Forms.MenuItem();
            this.miVerdana = new System.Windows.Forms.MenuItem();
            this.tbbFontSize = new System.Windows.Forms.ToolBarButton();
            this.cmFontSizes = new System.Windows.Forms.ContextMenu();
            this.mi7 = new System.Windows.Forms.MenuItem();
            this.mi8 = new System.Windows.Forms.MenuItem();
            this.mi9 = new System.Windows.Forms.MenuItem();
            this.mi10 = new System.Windows.Forms.MenuItem();
            this.mi11 = new System.Windows.Forms.MenuItem();
            this.mi12 = new System.Windows.Forms.MenuItem();
            this.mi13 = new System.Windows.Forms.MenuItem();
            this.mi14 = new System.Windows.Forms.MenuItem();
            this.mi16 = new System.Windows.Forms.MenuItem();
            this.mi18 = new System.Windows.Forms.MenuItem();
            this.mi20 = new System.Windows.Forms.MenuItem();
            this.mi22 = new System.Windows.Forms.MenuItem();
            this.mi24 = new System.Windows.Forms.MenuItem();
            this.mi26 = new System.Windows.Forms.MenuItem();
            this.mi28 = new System.Windows.Forms.MenuItem();
            this.mi36 = new System.Windows.Forms.MenuItem();
            this.mi48 = new System.Windows.Forms.MenuItem();
            this.mi72 = new System.Windows.Forms.MenuItem();
            this.tbbSeparator5 = new System.Windows.Forms.ToolBarButton();
            this.tbbBold = new System.Windows.Forms.ToolBarButton();
            this.tbbItalic = new System.Windows.Forms.ToolBarButton();
            this.tbbUnderline = new System.Windows.Forms.ToolBarButton();
            this.tbbStrikeout = new System.Windows.Forms.ToolBarButton();
            this.tbbSeparator1 = new System.Windows.Forms.ToolBarButton();
            this.tbbLeft = new System.Windows.Forms.ToolBarButton();
            this.tbbCenter = new System.Windows.Forms.ToolBarButton();
            this.tbbRight = new System.Windows.Forms.ToolBarButton();
            this.tbbSeparator2 = new System.Windows.Forms.ToolBarButton();
            this.tbbUndo = new System.Windows.Forms.ToolBarButton();
            this.tbbRedo = new System.Windows.Forms.ToolBarButton();
            this.tbbSeparator4 = new System.Windows.Forms.ToolBarButton();
            this.tbbStamp = new System.Windows.Forms.ToolBarButton();
            this.tbbColor = new System.Windows.Forms.ToolBarButton();
            this.cmColors = new System.Windows.Forms.ContextMenu();
            this.miBlack = new System.Windows.Forms.MenuItem();
            this.miBlue = new System.Windows.Forms.MenuItem();
            this.miRed = new System.Windows.Forms.MenuItem();
            this.miGreen = new System.Windows.Forms.MenuItem();
            this.tbbSparator6 = new System.Windows.Forms.ToolBarButton();
            this.tbbSpell = new System.Windows.Forms.ToolBarButton();
            this.imgList1 = new System.Windows.Forms.ImageList(this.components);
            this.ofd1 = new System.Windows.Forms.OpenFileDialog();
            this.sfd1 = new System.Windows.Forms.SaveFileDialog();
            this.spelling = new NetSpell.SpellChecker.Spelling(this.components);
            this.wordDictionary = new NetSpell.SpellChecker.Dictionary.WordDictionary(this.components);
            this.rtb1 = new EMRControlLib.RichTextBoxEx();
            this.SuspendLayout();
            // 
            // tb1
            // 
            this.tb1.Appearance = System.Windows.Forms.ToolBarAppearance.Flat;
            this.tb1.Buttons.AddRange(new System.Windows.Forms.ToolBarButton[] {
            this.tbbSave,
            this.tbbOpen,
            this.tbbSeparator3,
            this.tbbFont,
            this.tbbFontSize,
            this.tbbSeparator5,
            this.tbbBold,
            this.tbbItalic,
            this.tbbUnderline,
            this.tbbStrikeout,
            this.tbbSeparator1,
            this.tbbLeft,
            this.tbbCenter,
            this.tbbRight,
            this.tbbSeparator2,
            this.tbbUndo,
            this.tbbRedo,
            this.tbbSeparator4,
            this.tbbStamp,
            this.tbbColor,
            this.tbbSparator6,
            this.tbbSpell});
            this.tb1.ButtonSize = new System.Drawing.Size(16, 16);
            this.tb1.Divider = false;
            this.tb1.DropDownArrows = true;
            this.tb1.ImageList = this.imgList1;
            this.tb1.Location = new System.Drawing.Point(0, 0);
            this.tb1.Name = "tb1";
            this.tb1.ShowToolTips = true;
            this.tb1.Size = new System.Drawing.Size(516, 26);
            this.tb1.TabIndex = 0;
            this.tb1.ButtonClick += new System.Windows.Forms.ToolBarButtonClickEventHandler(this.tb1_ButtonClick);
            // 
            // tbbSave
            // 
            this.tbbSave.ImageIndex = 11;
            this.tbbSave.Name = "tbbSave";
            this.tbbSave.ToolTipText = "Save";
            // 
            // tbbOpen
            // 
            this.tbbOpen.ImageIndex = 10;
            this.tbbOpen.Name = "tbbOpen";
            this.tbbOpen.ToolTipText = "Open";
            // 
            // tbbSeparator3
            // 
            this.tbbSeparator3.Name = "tbbSeparator3";
            this.tbbSeparator3.Style = System.Windows.Forms.ToolBarButtonStyle.Separator;
            // 
            // tbbFont
            // 
            this.tbbFont.DropDownMenu = this.cmFonts;
            this.tbbFont.ImageIndex = 14;
            this.tbbFont.Name = "tbbFont";
            this.tbbFont.Style = System.Windows.Forms.ToolBarButtonStyle.DropDownButton;
            this.tbbFont.ToolTipText = "Font";
            // 
            // cmFonts
            // 
            this.cmFonts.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.miArial,
            this.miCourierNew,
            this.miGaramond,
            this.miLucidaConsole,
            this.miMicrosoftSansSerif,
            this.miTahoma,
            this.miTimesNewRoman,
            this.miVerdana});
            // 
            // miArial
            // 
            this.miArial.Index = 0;
            this.miArial.Text = "Arial";
            this.miArial.Click += new System.EventHandler(this.Font_Click);
            // 
            // miCourierNew
            // 
            this.miCourierNew.Index = 1;
            this.miCourierNew.Text = "Courier New";
            this.miCourierNew.Click += new System.EventHandler(this.Font_Click);
            // 
            // miGaramond
            // 
            this.miGaramond.Index = 2;
            this.miGaramond.Text = "Garamond";
            this.miGaramond.Click += new System.EventHandler(this.Font_Click);
            // 
            // miLucidaConsole
            // 
            this.miLucidaConsole.Index = 3;
            this.miLucidaConsole.Text = "Lucida Console";
            this.miLucidaConsole.Click += new System.EventHandler(this.Font_Click);
            // 
            // miMicrosoftSansSerif
            // 
            this.miMicrosoftSansSerif.Index = 4;
            this.miMicrosoftSansSerif.Text = "Microsoft Sans Serif";
            this.miMicrosoftSansSerif.Click += new System.EventHandler(this.Font_Click);
            // 
            // miTahoma
            // 
            this.miTahoma.Index = 5;
            this.miTahoma.Text = "Tahoma";
            this.miTahoma.Click += new System.EventHandler(this.Font_Click);
            // 
            // miTimesNewRoman
            // 
            this.miTimesNewRoman.Index = 6;
            this.miTimesNewRoman.Text = "Times New Roman";
            this.miTimesNewRoman.Click += new System.EventHandler(this.Font_Click);
            // 
            // miVerdana
            // 
            this.miVerdana.Index = 7;
            this.miVerdana.Text = "Verdana";
            this.miVerdana.Click += new System.EventHandler(this.Font_Click);
            // 
            // tbbFontSize
            // 
            this.tbbFontSize.DropDownMenu = this.cmFontSizes;
            this.tbbFontSize.ImageIndex = 15;
            this.tbbFontSize.Name = "tbbFontSize";
            this.tbbFontSize.Style = System.Windows.Forms.ToolBarButtonStyle.DropDownButton;
            this.tbbFontSize.ToolTipText = "Font Size";
            // 
            // cmFontSizes
            // 
            this.cmFontSizes.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.mi7,
            this.mi8,
            this.mi9,
            this.mi10,
            this.mi11,
            this.mi12,
            this.mi13,
            this.mi14,
            this.mi16,
            this.mi18,
            this.mi20,
            this.mi22,
            this.mi24,
            this.mi26,
            this.mi28,
            this.mi36,
            this.mi48,
            this.mi72});
            // 
            // mi7
            // 
            this.mi7.Index = 0;
            this.mi7.Text = "7.0";
            this.mi7.Click += new System.EventHandler(this.FontSize_Click);
            // 
            // mi8
            // 
            this.mi8.Index = 1;
            this.mi8.Text = "8";
            this.mi8.Click += new System.EventHandler(this.FontSize_Click);
            // 
            // mi9
            // 
            this.mi9.Index = 2;
            this.mi9.Text = "9";
            this.mi9.Click += new System.EventHandler(this.FontSize_Click);
            // 
            // mi10
            // 
            this.mi10.Index = 3;
            this.mi10.Text = "10";
            this.mi10.Click += new System.EventHandler(this.FontSize_Click);
            // 
            // mi11
            // 
            this.mi11.Index = 4;
            this.mi11.Text = "11";
            this.mi11.Click += new System.EventHandler(this.FontSize_Click);
            // 
            // mi12
            // 
            this.mi12.Index = 5;
            this.mi12.Text = "12";
            this.mi12.Click += new System.EventHandler(this.FontSize_Click);
            // 
            // mi13
            // 
            this.mi13.Index = 6;
            this.mi13.Text = "13";
            this.mi13.Click += new System.EventHandler(this.FontSize_Click);
            // 
            // mi14
            // 
            this.mi14.Index = 7;
            this.mi14.Text = "14";
            this.mi14.Click += new System.EventHandler(this.FontSize_Click);
            // 
            // mi16
            // 
            this.mi16.Index = 8;
            this.mi16.Text = "16";
            this.mi16.Click += new System.EventHandler(this.FontSize_Click);
            // 
            // mi18
            // 
            this.mi18.Index = 9;
            this.mi18.Text = "18";
            this.mi18.Click += new System.EventHandler(this.FontSize_Click);
            // 
            // mi20
            // 
            this.mi20.Index = 10;
            this.mi20.Text = "20";
            this.mi20.Click += new System.EventHandler(this.FontSize_Click);
            // 
            // mi22
            // 
            this.mi22.Index = 11;
            this.mi22.Text = "22";
            this.mi22.Click += new System.EventHandler(this.FontSize_Click);
            // 
            // mi24
            // 
            this.mi24.Index = 12;
            this.mi24.Text = "24";
            this.mi24.Click += new System.EventHandler(this.FontSize_Click);
            // 
            // mi26
            // 
            this.mi26.Index = 13;
            this.mi26.Text = "26";
            this.mi26.Click += new System.EventHandler(this.FontSize_Click);
            // 
            // mi28
            // 
            this.mi28.Index = 14;
            this.mi28.Text = "28";
            this.mi28.Click += new System.EventHandler(this.FontSize_Click);
            // 
            // mi36
            // 
            this.mi36.Index = 15;
            this.mi36.Text = "36";
            this.mi36.Click += new System.EventHandler(this.FontSize_Click);
            // 
            // mi48
            // 
            this.mi48.Index = 16;
            this.mi48.Text = "48";
            this.mi48.Click += new System.EventHandler(this.FontSize_Click);
            // 
            // mi72
            // 
            this.mi72.Index = 17;
            this.mi72.Text = "72";
            this.mi72.Click += new System.EventHandler(this.FontSize_Click);
            // 
            // tbbSeparator5
            // 
            this.tbbSeparator5.Name = "tbbSeparator5";
            this.tbbSeparator5.Style = System.Windows.Forms.ToolBarButtonStyle.Separator;
            // 
            // tbbBold
            // 
            this.tbbBold.ImageIndex = 0;
            this.tbbBold.Name = "tbbBold";
            this.tbbBold.Style = System.Windows.Forms.ToolBarButtonStyle.ToggleButton;
            this.tbbBold.ToolTipText = "Bold";
            // 
            // tbbItalic
            // 
            this.tbbItalic.ImageIndex = 1;
            this.tbbItalic.Name = "tbbItalic";
            this.tbbItalic.Style = System.Windows.Forms.ToolBarButtonStyle.ToggleButton;
            this.tbbItalic.ToolTipText = "Italic";
            // 
            // tbbUnderline
            // 
            this.tbbUnderline.ImageIndex = 2;
            this.tbbUnderline.Name = "tbbUnderline";
            this.tbbUnderline.Style = System.Windows.Forms.ToolBarButtonStyle.ToggleButton;
            this.tbbUnderline.ToolTipText = "Underline";
            // 
            // tbbStrikeout
            // 
            this.tbbStrikeout.ImageIndex = 3;
            this.tbbStrikeout.Name = "tbbStrikeout";
            this.tbbStrikeout.ToolTipText = "Strikeout";
            // 
            // tbbSeparator1
            // 
            this.tbbSeparator1.Name = "tbbSeparator1";
            this.tbbSeparator1.Style = System.Windows.Forms.ToolBarButtonStyle.Separator;
            // 
            // tbbLeft
            // 
            this.tbbLeft.ImageIndex = 4;
            this.tbbLeft.Name = "tbbLeft";
            this.tbbLeft.ToolTipText = "Left";
            // 
            // tbbCenter
            // 
            this.tbbCenter.ImageIndex = 5;
            this.tbbCenter.Name = "tbbCenter";
            this.tbbCenter.Style = System.Windows.Forms.ToolBarButtonStyle.ToggleButton;
            this.tbbCenter.ToolTipText = "Center";
            // 
            // tbbRight
            // 
            this.tbbRight.ImageIndex = 6;
            this.tbbRight.Name = "tbbRight";
            this.tbbRight.Style = System.Windows.Forms.ToolBarButtonStyle.ToggleButton;
            this.tbbRight.ToolTipText = "Right";
            // 
            // tbbSeparator2
            // 
            this.tbbSeparator2.Name = "tbbSeparator2";
            this.tbbSeparator2.Style = System.Windows.Forms.ToolBarButtonStyle.Separator;
            // 
            // tbbUndo
            // 
            this.tbbUndo.ImageIndex = 12;
            this.tbbUndo.Name = "tbbUndo";
            this.tbbUndo.ToolTipText = "Undo";
            // 
            // tbbRedo
            // 
            this.tbbRedo.ImageIndex = 13;
            this.tbbRedo.Name = "tbbRedo";
            this.tbbRedo.ToolTipText = "Redo";
            // 
            // tbbSeparator4
            // 
            this.tbbSeparator4.Name = "tbbSeparator4";
            this.tbbSeparator4.Style = System.Windows.Forms.ToolBarButtonStyle.Separator;
            // 
            // tbbStamp
            // 
            this.tbbStamp.Enabled = false;
            this.tbbStamp.ImageIndex = 8;
            this.tbbStamp.Name = "tbbStamp";
            this.tbbStamp.ToolTipText = "Edit Stamp";
            // 
            // tbbColor
            // 
            this.tbbColor.DropDownMenu = this.cmColors;
            this.tbbColor.ImageIndex = 7;
            this.tbbColor.Name = "tbbColor";
            this.tbbColor.Style = System.Windows.Forms.ToolBarButtonStyle.DropDownButton;
            this.tbbColor.ToolTipText = "Color";
            // 
            // cmColors
            // 
            this.cmColors.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.miBlack,
            this.miBlue,
            this.miRed,
            this.miGreen});
            // 
            // miBlack
            // 
            this.miBlack.Index = 0;
            this.miBlack.Text = "Black";
            this.miBlack.Click += new System.EventHandler(this.Color_Click);
            // 
            // miBlue
            // 
            this.miBlue.Index = 1;
            this.miBlue.Text = "Blue";
            this.miBlue.Click += new System.EventHandler(this.Color_Click);
            // 
            // miRed
            // 
            this.miRed.Index = 2;
            this.miRed.Text = "Red";
            this.miRed.Click += new System.EventHandler(this.Color_Click);
            // 
            // miGreen
            // 
            this.miGreen.Index = 3;
            this.miGreen.Text = "Green";
            this.miGreen.Click += new System.EventHandler(this.Color_Click);
            // 
            // tbbSparator6
            // 
            this.tbbSparator6.Name = "tbbSparator6";
            this.tbbSparator6.Style = System.Windows.Forms.ToolBarButtonStyle.Separator;
            // 
            // tbbSpell
            // 
            this.tbbSpell.ImageIndex = 17;
            this.tbbSpell.Name = "tbbSpell";
            this.tbbSpell.ToolTipText = "SpellCheck";
            // 
            // imgList1
            // 
            this.imgList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imgList1.ImageStream")));
            this.imgList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imgList1.Images.SetKeyName(0, "");
            this.imgList1.Images.SetKeyName(1, "");
            this.imgList1.Images.SetKeyName(2, "");
            this.imgList1.Images.SetKeyName(3, "");
            this.imgList1.Images.SetKeyName(4, "");
            this.imgList1.Images.SetKeyName(5, "");
            this.imgList1.Images.SetKeyName(6, "");
            this.imgList1.Images.SetKeyName(7, "");
            this.imgList1.Images.SetKeyName(8, "");
            this.imgList1.Images.SetKeyName(9, "");
            this.imgList1.Images.SetKeyName(10, "");
            this.imgList1.Images.SetKeyName(11, "");
            this.imgList1.Images.SetKeyName(12, "");
            this.imgList1.Images.SetKeyName(13, "");
            this.imgList1.Images.SetKeyName(14, "");
            this.imgList1.Images.SetKeyName(15, "");
            this.imgList1.Images.SetKeyName(16, "");
            this.imgList1.Images.SetKeyName(17, "");
            // 
            // ofd1
            // 
            this.ofd1.DefaultExt = "rtf";
            this.ofd1.Filter = "Rich Text Files|*.rtf|Plain Text File|*.txt";
            this.ofd1.Title = "Open File";
            // 
            // sfd1
            // 
            this.sfd1.DefaultExt = "rtf";
            this.sfd1.Filter = "Rich Text File|*.rtf|Plain Text File|*.txt";
            this.sfd1.Title = "Save As";
            // 
            // spelling
            // 
            this.spelling.Dictionary = this.wordDictionary;
            this.spelling.DeletedWord += new NetSpell.SpellChecker.Spelling.DeletedWordEventHandler(this.spelling_DeletedWord);
            this.spelling.ReplacedWord += new NetSpell.SpellChecker.Spelling.ReplacedWordEventHandler(this.spelling_ReplacedWord);
            // 
            // wordDictionary
            // 
            this.wordDictionary.DictionaryFolder = "Dic";
            // 
            // rtb1
            // 
            this.rtb1.DetectUrls = false;
            this.rtb1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtb1.HiglightColor = EMRControlLib.RichTextBoxEx.RtfColor.White;
            this.rtb1.Location = new System.Drawing.Point(0, 26);
            this.rtb1.Name = "rtb1";
            this.rtb1.Size = new System.Drawing.Size(516, 198);
            this.rtb1.TabIndex = 1;
            this.rtb1.Text = "";
            this.rtb1.TextColor = EMRControlLib.RichTextBoxEx.RtfColor.Black;
            this.rtb1.LinkClicked += new System.Windows.Forms.LinkClickedEventHandler(this.rtb1_LinkClicked);
            this.rtb1.SelectionChanged += new System.EventHandler(this.rtb1_SelectionChanged);
            // 
            // EMRRichTextBox
            // 
            this.Controls.Add(this.rtb1);
            this.Controls.Add(this.tb1);
            this.Name = "EMRRichTextBox";
            this.Size = new System.Drawing.Size(516, 224);
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion

		#region Stamp Event Stuff
		[Description("Occurs when the stamp button is clicked"), 
		 Category("Behavior")]
		public event System.EventHandler Stamp;
		
		/// <summary>
		/// OnStamp event
		/// </summary>
		protected virtual void OnStamp(EventArgs e)
		{
			if(Stamp != null)
				Stamp(this, e);

			switch(StampAction)
			{
				case StampActions.EditedBy:
				{
					StringBuilder stamp = new StringBuilder(""); //holds our stamp text
					if(rtb1.Text.Length > 0) stamp.Append("\r\n\r\n"); //add two lines for space
					stamp.Append("Edited by "); 
					//use the CurrentPrincipal name if one exsist else use windows logon username
					if(Thread.CurrentPrincipal == null || Thread.CurrentPrincipal.Identity == null || Thread.CurrentPrincipal.Identity.Name.Length <= 0)
						stamp.Append(Environment.UserName);
					else
						stamp.Append(Thread.CurrentPrincipal.Identity.Name);
					stamp.Append(" on " + DateTime.Now.ToLongDateString() + "\r\n");
			
					rtb1.SelectionLength = 0; //unselect everything basicly
					rtb1.SelectionStart = rtb1.Text.Length; //start new selection at the end of the text
					rtb1.SelectionColor = this.StampColor; //make the selection blue
					rtb1.SelectionFont = new Font(rtb1.SelectionFont, FontStyle.Bold); //set the selection font and style
					rtb1.AppendText(stamp.ToString()); //add the stamp to the richtextbox
					rtb1.Focus(); //set focus back on the richtextbox
				} break; //end edited by stamp
				case StampActions.DateTime:
				{
					StringBuilder stamp = new StringBuilder(""); //holds our stamp text
					if(rtb1.Text.Length > 0) stamp.Append("\r\n\r\n"); //add two lines for space
					stamp.Append(DateTime.Now.ToLongDateString() + "\r\n");
					rtb1.SelectionLength = 0; //unselect everything basicly
					rtb1.SelectionStart = rtb1.Text.Length; //start new selection at the end of the text
					rtb1.SelectionColor = this.StampColor; //make the selection blue
					rtb1.SelectionFont = new Font(rtb1.SelectionFont, FontStyle.Bold); //set the selection font and style
					rtb1.AppendText(stamp.ToString()); //add the stamp to the richtextbox
					rtb1.Focus(); //set focus back on the richtextbox
				} break;
			} //end select
		}
		#endregion

		#region Toolbar button click
		public void openButtonClick(ButtonType bt)
		{
			switch(bt)
			{
				case ButtonType.Open:
					tb1_ButtonClick(this,new ToolBarButtonClickEventArgs(tbbOpen));
					break;
				case ButtonType.Save:
					tb1_ButtonClick(this,new ToolBarButtonClickEventArgs(tbbSave));
					break;
				case ButtonType.Text:
					tb1_ButtonClick(this,new ToolBarButtonClickEventArgs(tbbFont));
					break;
				case ButtonType.TextSize:
					tb1_ButtonClick(this,new ToolBarButtonClickEventArgs(tbbFontSize));
					break;
				case ButtonType.Bold:
					tb1_ButtonClick(this,new ToolBarButtonClickEventArgs(tbbBold));
					break;
				case ButtonType.Italic:
					tb1_ButtonClick(this,new ToolBarButtonClickEventArgs(tbbItalic));
					break;
				case ButtonType.Underline:
					tb1_ButtonClick(this,new ToolBarButtonClickEventArgs(tbbUnderline));
					break;
				case ButtonType.Strike:
					tb1_ButtonClick(this,new ToolBarButtonClickEventArgs(tbbStrikeout));
					break;
				case ButtonType.Left:
					tb1_ButtonClick(this,new ToolBarButtonClickEventArgs(tbbLeft));
					break;
				case ButtonType.Center:
					tb1_ButtonClick(this,new ToolBarButtonClickEventArgs(tbbCenter));
					break;
				case ButtonType.Right:
					tb1_ButtonClick(this,new ToolBarButtonClickEventArgs(tbbRight));
					break;
				case ButtonType.Undo:
					tb1_ButtonClick(this,new ToolBarButtonClickEventArgs(tbbUndo));
					break;
				case ButtonType.Redo:
					tb1_ButtonClick(this,new ToolBarButtonClickEventArgs(tbbRedo));
					break;
				case ButtonType.Stamp:
					tb1_ButtonClick(this,new ToolBarButtonClickEventArgs(tbbStamp));
					break;
				case ButtonType.Color:
					tb1_ButtonClick(this,new ToolBarButtonClickEventArgs(tbbColor));
					break;
				case ButtonType.Spell:
					tb1_ButtonClick(this,new ToolBarButtonClickEventArgs(tbbSpell));
					break;
			}
		}
		/// <summary>
		///     Handler for the toolbar button click event
		/// </summary>
		private void tb1_ButtonClick(object sender, System.Windows.Forms.ToolBarButtonClickEventArgs e)
		{
			//Switch based on the tooltip of the button pressed
			//OR: This could be changed to switch on the actual button pressed (e.g. e.Button and the case would be tbbBold)
			switch(e.Button.ToolTipText.ToLower())
			{
				case "bold": 
				{
					if(rtb1.SelectionFont != null) 
					{
						//using bitwise Exclusive OR to flip-flop the value
						rtb1.SelectionFont = new Font(rtb1.SelectionFont, rtb1.SelectionFont.Style ^ FontStyle.Bold);
					}
				} break;
				case "italic": 
				{
					if(rtb1.SelectionFont != null) 
					{
						//using bitwise Exclusive OR to flip-flop the value
						rtb1.SelectionFont = new Font(rtb1.SelectionFont, rtb1.SelectionFont.Style ^ FontStyle.Italic);
					}
				} break;
				case "underline":
				{
					if(rtb1.SelectionFont != null) 
					{
						//using bitwise Exclusive OR to flip-flop the value
						rtb1.SelectionFont = new Font(rtb1.SelectionFont, rtb1.SelectionFont.Style ^ FontStyle.Underline);
					}
				} break;
				case "strikeout":
				{
					if(rtb1.SelectionFont != null) 
					{
						//using bitwise Exclusive OR to flip-flop the value
						rtb1.SelectionFont = new Font(rtb1.SelectionFont, rtb1.SelectionFont.Style ^ FontStyle.Strikeout);
					}
				} break;
				case "left":
				{
					//change horizontal alignment to left
					rtb1.SelectionAlignment = HorizontalAlignment.Left; 
				} break;
				case "center":
				{
					//change horizontal alignment to center
					rtb1.SelectionAlignment = HorizontalAlignment.Center;
				} break;
				case "right":
				{
					//change horizontal alignment to right
					rtb1.SelectionAlignment = HorizontalAlignment.Right;
				} break;
				case "edit stamp":
				{
					OnStamp(new EventArgs()); //send stamp event
				} break;
				case "color":
				{
					rtb1.SelectionColor = Color.Black;
				} break;
				case "undo":
				{
					rtb1.Undo();
				} break;
				case "redo":
				{
					rtb1.Redo();
				} break;
				case "open":
				{
					try
					{
						if (ofd1.ShowDialog() == DialogResult.OK && ofd1.FileName.Length > 0)
							if(System.IO.Path.GetExtension(ofd1.FileName).ToLower().Equals(".rtf")) 
								rtb1.LoadFile(ofd1.FileName, RichTextBoxStreamType.RichText);
							else
								rtb1.LoadFile(ofd1.FileName, RichTextBoxStreamType.PlainText);
					}
					catch (ArgumentException ae)
					{
						if(ae.Message == "Invalid file format.")
							DevExpress.XtraEditors.XtraMessageBox.Show("There was an error loading the file: " + ofd1.FileName);				
					}
				} break;
				case "save":
				{
					if(sfd1.ShowDialog() == DialogResult.OK && sfd1.FileName.Length > 0)
						if(System.IO.Path.GetExtension(sfd1.FileName).ToLower().Equals(".rtf"))
							rtb1.SaveFile(sfd1.FileName);
						else
							rtb1.SaveFile(sfd1.FileName, RichTextBoxStreamType.PlainText);
				} break;
				case "spellcheck":
                    try
                    {
                        string s = Directory.GetCurrentDirectory();
                        Directory.SetCurrentDirectory(Application.StartupPath);
                        spelling.Text = rtb1.Text;
                        spelling.SpellCheck();
                        Directory.SetCurrentDirectory(s);
                    }
                    catch ( Exception ex)
                    {
                        MessageBox.Show("***ERROR*** " + ex.Message.ToString());
                    }
                    break;
            } //end select
            UpdateToolbar(); //Update the toolbar buttons
		}
		#endregion

		#region Update Toolbar
		/// <summary>
		///     Update the toolbar button statuses
		/// </summary>
		public void UpdateToolbar()
		{
			//This is done incase 2 different fonts are selected at the same time
			//If that is the case there is no selection font so I use the default
			//font instead.
			Font fnt;
			if(rtb1.SelectionFont != null)
				fnt = rtb1.SelectionFont;
			else
				fnt = rtb1.Font;

			//Do all the toolbar button checks
			tbbBold.Pushed		= fnt.Bold; //bold button
			tbbItalic.Pushed	= fnt.Italic; //italic button
			tbbUnderline.Pushed	= fnt.Underline; //underline button
			tbbStrikeout.Pushed	= fnt.Strikeout; //strikeout button
			tbbLeft.Pushed		= (rtb1.SelectionAlignment == HorizontalAlignment.Left); //justify left
			tbbCenter.Pushed	= (rtb1.SelectionAlignment == HorizontalAlignment.Center); //justify center
			tbbRight.Pushed		= (rtb1.SelectionAlignment == HorizontalAlignment.Right); //justify right

			//Check the correct color
			foreach(MenuItem mi in cmColors.MenuItems)
			{
				mi.Checked = (rtb1.SelectionColor == Color.FromName(mi.Text));
			}

			//Check the correct font
			foreach(MenuItem mi in cmFonts.MenuItems)
			{
				mi.Checked = (fnt.FontFamily.Name == mi.Text);
			}

			//Check the correct font size
			foreach(MenuItem mi in cmFontSizes.MenuItems)
			{
				mi.Checked = ((int)fnt.SizeInPoints == float.Parse(mi.Text));
			}
		}
		#endregion

		#region Update Toolbar Seperators
		private void UpdateToolbarSeperators()
		{
			//Save & Open
			if(!tbbSave.Visible && !tbbOpen.Visible) 
				tbbSeparator3.Visible = false;
			else
				tbbSeparator3.Visible = true;

			//Font & Font Size
			if(!tbbFont.Visible && !tbbFontSize.Visible) 
				tbbSeparator5.Visible = false;
			else
				tbbSeparator5.Visible = true;

			//Bold, Italic, Underline, & Strikeout
			if(!tbbBold.Visible && !tbbItalic.Visible && !tbbUnderline.Visible && !tbbStrikeout.Visible)
				tbbSeparator1.Visible = false;
			else
				tbbSeparator1.Visible = true;

			//Left, Center, & Right
			if(!tbbLeft.Visible && !tbbCenter.Visible && !tbbRight.Visible)
				tbbSeparator2.Visible = false;
			else
				tbbSeparator2.Visible = true;

			//Undo & Redo
			if(!tbbUndo.Visible && !tbbRedo.Visible) 
				tbbSeparator4.Visible = false;
			else
				tbbSeparator4.Visible = true;
		}
#endregion

		#region RichTextBox Selection Change
		/// <summary>
		///		Change the toolbar buttons when new text is selected
		/// </summary>
		private void rtb1_SelectionChanged(object sender, System.EventArgs e)
		{
			UpdateToolbar(); //Update the toolbar buttons
		}
#endregion

		#region Color Click
		/// <summary>
		///     Change the richtextbox color
		/// </summary>
		private void Color_Click(object sender, System.EventArgs e)
		{
				//set the richtextbox color based on the name of the menu item
				rtb1.SelectionColor = Color.FromName(((MenuItem)sender).Text);

		}
		#endregion

		#region Font Click
		/// <summary>
		///     Change the richtextbox font
		/// </summary>
		private void Font_Click(object sender, System.EventArgs e)
		{
			if(rtb1.SelectionFont != null) 
			{
				//set the richtextbox font family based on the name of the menu item
				rtb1.SelectionFont = new Font(((MenuItem)sender).Text, rtb1.SelectionFont.SizeInPoints);
			}
			else
				rtb1.SelectionFont = new Font(((MenuItem)sender).Text, rtb1.Font.SizeInPoints);
		}
		#endregion

		#region Font Size Click
		/// <summary>
		///     Change the richtextbox font size
		/// </summary>
		private void FontSize_Click(object sender, System.EventArgs e)
		{
			if(rtb1.SelectionFont != null)
			{
				//set the richtextbox font size based on the name of the menu item
				rtb1.SelectionFont = new Font(rtb1.SelectionFont.FontFamily.Name, float.Parse(((MenuItem)sender).Text));
			}
			else
				rtb1.SelectionFont = new Font(rtb1.Font.Name, float.Parse(((MenuItem)sender).Text));

		}
		#endregion

		#region Link Clicked
		/// <summary>
		/// Starts the default browser if a link is clicked
		/// </summary>
		private void rtb1_LinkClicked(object sender, System.Windows.Forms.LinkClickedEventArgs e)
		{
			try
			{
			System.Diagnostics.Process.Start(e.LinkText);
		}
			catch
			{ // Can't find file! Get out!
			}
		}
		#endregion

		#region Public Properties
		/// <summary>
		///     The toolbar that is contained with-in the RichTextBoxExtened control
		/// </summary>
		[Description("The internal toolbar control"),
		Category("Internal Controls")]
		public ToolBar Toolbar
		{
			get { return tb1; }
		}

		/// <summary>
		///     The RichTextBox that is contained with-in the RichTextBoxExtened control
		/// </summary>
		[Description("The internal richtextbox control"),
		Category("Internal Controls")]
		public RichTextBoxEx RichTextBox
		{
			get	{ return rtb1; }
		}

		/// <summary>
		///     Show the save button or not
		/// </summary>
		[Description("Show the save button or not"),
		Category("Appearance")]
		public Boolean ShowSave
		{
			get { return tbbSave.Visible; }
			set { tbbSave.Visible = value; UpdateToolbarSeperators(); }
		}

		/// <summary>
		///    Show the open button or not 
		/// </summary>
		[Description("Show the open button or not"),
		Category("Appearance")]
		public Boolean ShowOpen
		{
			get { return tbbOpen.Visible; }
			set	{ tbbOpen.Visible = value; UpdateToolbarSeperators(); }
		}

		/// <summary>
		///     Show the stamp button or not
		/// </summary>
		[Description("Show the stamp button or not"),
		 Category("Appearance")]
		public Boolean ShowStamp
		{
			get { return tbbStamp.Visible; }
			set { tbbStamp.Visible = value; }
		}
		
		/// <summary>
		///     Show the color button or not
		/// </summary>
		[Description("Show the color button or not"),
		Category("Appearance")]
		public Boolean ShowColors
		{
			get { return tbbColor.Visible; }
			set { tbbColor.Visible = value; }
		}

		/// <summary>
		///     Show the undo button or not
		/// </summary>
		[Description("Show the undo button or not"),
		Category("Appearance")]
		public Boolean ShowUndo
		{
			get { return tbbUndo.Visible; }
			set { tbbUndo.Visible = value; UpdateToolbarSeperators(); }
		}

		/// <summary>
		///     Show the redo button or not
		/// </summary>
		[Description("Show the redo button or not"),
		Category("Appearance")]
		public Boolean ShowRedo
		{
			get { return tbbRedo.Visible; }
			set { tbbRedo.Visible = value; UpdateToolbarSeperators(); }
		}

		/// <summary>
		///     Show the bold button or not
		/// </summary>
		[Description("Show the bold button or not"),
		Category("Appearance")]
		public Boolean ShowBold
		{
			get { return tbbBold.Visible; }
			set { tbbBold.Visible = value; UpdateToolbarSeperators(); }
		}

		/// <summary>
		///     Show the italic button or not
		/// </summary>
		[Description("Show the italic button or not"),
		Category("Appearance")]
		public Boolean ShowItalic
		{
			get { return tbbItalic.Visible; }
			set { tbbItalic.Visible = value; UpdateToolbarSeperators(); }
		}

		/// <summary>
		///     Show the underline button or not
		/// </summary>
		[Description("Show the underline button or not"),
		Category("Appearance")]
		public Boolean ShowUnderline
		{
			get { return tbbUnderline.Visible; }
			set { tbbUnderline.Visible = value; UpdateToolbarSeperators(); }
		}

		/// <summary>
		///     Show the strikeout button or not
		/// </summary>
		[Description("Show the strikeout button or not"),
		Category("Appearance")]
		public Boolean ShowStrikeout
		{
			get { return tbbStrikeout.Visible; }
			set { tbbStrikeout.Visible = value; UpdateToolbarSeperators(); }
		}

		/// <summary>
		///     Show the left justify button or not
		/// </summary>
		[Description("Show the left justify button or not"),
		Category("Appearance")]
		public Boolean ShowLeftJustify
		{
			get { return tbbLeft.Visible; }
			set { tbbLeft.Visible = value; UpdateToolbarSeperators(); }
		}

		/// <summary>
		///     Show the right justify button or not
		/// </summary>
		[Description("Show the right justify button or not"),
		Category("Appearance")]
		public Boolean ShowRightJustify
		{
			get { return tbbRight.Visible; }
			set { tbbRight.Visible = value; UpdateToolbarSeperators(); }
		}

		/// <summary>
		///     Show the center justify button or not
		/// </summary>
		[Description("Show the center justify button or not"),
		Category("Appearance")]
		public Boolean ShowCenterJustify
		{
			get { return tbbCenter.Visible; }
			set { tbbCenter.Visible = value; UpdateToolbarSeperators(); }
		}

		/// <summary>
		///     Determines how the stamp button will respond
		/// </summary>
		StampActions m_StampAction = StampActions.EditedBy;
		[Description("Determines how the stamp button will respond"),
		Category("Behavior")]
		public StampActions StampAction
		{
			get { return m_StampAction; }
			set { m_StampAction = value; }
		}
		
		/// <summary>
		///     Color of the stamp text
		/// </summary>
		Color m_StampColor = Color.Blue;

		[Description("Color of the stamp text"),
		Category("Appearance")]
		public Color StampColor
		{
			get { return m_StampColor; }
			set { m_StampColor = value; }
		}
			
		/// <summary>
		///     Show the font button or not
		/// </summary>
		[Description("Show the font button or not"),
		Category("Appearance")]
		public Boolean ShowFont
		{
			get { return tbbFont.Visible; }
			set { tbbFont.Visible = value; }
		}

		/// <summary>
		///     Show the font size button or not
		/// </summary>
		[Description("Show the font size button or not"),
		Category("Appearance")]
		public Boolean ShowFontSize
		{
			get { return tbbFontSize.Visible; }
			set { tbbFontSize.Visible = value; }
		}

		/// <summary>
		///     Detect URLs with-in the richtextbox
		/// </summary>
		[Description("Detect URLs with-in the richtextbox"),
		Category("Behavior")]
		public Boolean DetectURLs
		{
			get { return rtb1.DetectUrls; }
			set { rtb1.DetectUrls = value; }
		}
		#endregion

		#region Spelling Events
		private void spelling_ReplacedWord(object sender, NetSpell.SpellChecker.ReplaceWordEventArgs e)
		{
            try
            {
                int start = rtb1.SelectionStart;
                int length = rtb1.SelectionLength;

                rtb1.Select(e.TextIndex, e.Word.Length);
                rtb1.SelectedText = e.ReplacementWord;

                if (start > rtb1.Text.Length)
                    start = rtb1.Text.Length;

                if ((start + length) > rtb1.Text.Length)
                    length = 0;

                rtb1.Select(start, length);
            }
            catch ( Exception ex)
            {

            }
		}

		private void spelling_DeletedWord(object sender, NetSpell.SpellChecker.SpellingEventArgs e)
		{
            try
            {
                int start = rtb1.SelectionStart;
                int length = rtb1.SelectionLength;

                rtb1.Select(e.TextIndex, e.Word.Length);
                rtb1.SelectedText = "";

                if (start > rtb1.Text.Length)
                    start = rtb1.Text.Length;

                if ((start + length) > rtb1.Text.Length)
                    length = 0;

                rtb1.Select(start, length);
            }
            catch ( Exception ex)
            {

            }
		}
		#endregion

	} //end class

} //end namespace
