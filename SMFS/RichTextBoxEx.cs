using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Printing;
using System.Drawing.Imaging;
using System.Data;
using System.Windows.Forms;
using System.Threading;
using System.Text;
using System.Runtime.InteropServices;
using System.Collections.Specialized;
using System.IO;
using GeneralLib;
using System.Reflection;

namespace EMRControlLib
{
	public delegate void EMRDragEventHandler(object sender, EMRDragEventArgs e);
	/// <summary>
	/// Summary description for RichTextBoxEx.
	/// </summary>
	public class RichTextBoxEx : System.Windows.Forms.RichTextBox
	{
        private System.ComponentModel.IContainer components;
		private DragDropEffects ddeLocal = DragDropEffects.All;
		private bool _moving = false;
		private int _localindex = -1;

		[DllImport("kernel32.dll", CharSet=CharSet.Auto)]
		static extern IntPtr LoadLibrary(string lpFileName);

#if(!DEBUG)
		/// <summary>
		/// Override the CreateParams method to use the newest richedit control available(For Viewing Tables Correctly)
		/// </summary>
		protected override CreateParams CreateParams
		{
			get
			{
				CreateParams parameters = base.CreateParams;

				if (LoadLibrary("msftedit.dll") != IntPtr.Zero)
					parameters.ClassName = "RICHEDIT50W";
				return parameters;
			}
		}
#endif

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
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

			ReleaseTemp();
			rtfColor = null;
		}

		public void ReleaseTemp()
		{
			try
			{
				if (File.Exists(_TemporaryFile))
					File.Delete(_TemporaryFile);
			}
			catch
			{
			}
		}

		#region Public Properties
		/// <summary>
		///     Show the save button or not
		/// </summary>
		[Description("Enable Scribble Function"),
		Category("Scribble")]
		#endregion

		#region Interop-Defines
		[StructLayout(LayoutKind.Sequential)]
		private struct CHARFORMAT2_STRUCT
		{
			public UInt32 cbSize;
			public UInt32 dwMask;
			public UInt32 dwEffects;
			public Int32 yHeight;
			public Int32 yOffset;
			public Int32 crTextColor;
			public byte bCharSet;
			public byte bPitchAndFamily;
			[MarshalAs(UnmanagedType.ByValArray, SizeConst=32)]
			public char[] szFaceName;
			public UInt16 wWeight;
			public UInt16 sSpacing;
			public int crBackColor; // Color.ToArgb() -> int
			public int lcid;
			public int dwReserved;
			public Int16 sStyle;
			public Int16 wKerning;
			public byte bUnderlineType;
			public byte bAnimation;
			public byte bRevAuthor;
			public byte bReserved1;
		}

		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

		private const int WM_USER			 = 0x0400;
		private const int EM_GETCHARFORMAT	 = WM_USER+58;
		private const int EM_SETCHARFORMAT	 = WM_USER+68;

		private const int SCF_SELECTION	= 0x0001;
		private const int SCF_WORD		= 0x0002;
		private const int SCF_ALL		= 0x0004;

		#region CHARFORMAT2 Flags
		private const UInt32 CFE_BOLD		= 0x0001;
		private const UInt32 CFE_ITALIC		= 0x0002;
		private const UInt32 CFE_UNDERLINE	= 0x0004;
		private const UInt32 CFE_STRIKEOUT	= 0x0008;
		private const UInt32 CFE_PROTECTED	= 0x0010;
		private const UInt32 CFE_LINK		= 0x0020;
		private const UInt32 CFE_AUTOCOLOR	= 0x40000000;
		private const UInt32 CFE_SUBSCRIPT	= 0x00010000;		/* Superscript and subscript are */
		private const UInt32 CFE_SUPERSCRIPT= 0x00020000;		/*  mutually exclusive			 */

		private const int CFM_SMALLCAPS		= 0x0040;			/* (*)	*/
		private const int CFM_ALLCAPS		= 0x0080;			/* Displayed by 3.0	*/
		private const int CFM_HIDDEN		= 0x0100;			/* Hidden by 3.0 */
		private const int CFM_OUTLINE		= 0x0200;			/* (*)	*/
		private const int CFM_SHADOW		= 0x0400;			/* (*)	*/
		private const int CFM_EMBOSS		= 0x0800;			/* (*)	*/
		private const int CFM_IMPRINT		= 0x1000;			/* (*)	*/
		private const int CFM_DISABLED		= 0x2000;
		private const int CFM_REVISED		= 0x4000;

		private const int CFM_BACKCOLOR		= 0x04000000;
		private const int CFM_LCID			= 0x02000000;
		private const int CFM_UNDERLINETYPE	= 0x00800000;		/* Many displayed by 3.0 */
		private const int CFM_WEIGHT		= 0x00400000;
		private const int CFM_SPACING		= 0x00200000;		/* Displayed by 3.0	*/
		private const int CFM_KERNING		= 0x00100000;		/* (*)	*/
		private const int CFM_STYLE			= 0x00080000;		/* (*)	*/
		private const int CFM_ANIMATION		= 0x00040000;		/* (*)	*/
		private const int CFM_REVAUTHOR		= 0x00008000;


		private const UInt32 CFM_BOLD		= 0x00000001;
		private const UInt32 CFM_ITALIC		= 0x00000002;
		private const UInt32 CFM_UNDERLINE	= 0x00000004;
		private const UInt32 CFM_STRIKEOUT	= 0x00000008;
		private const UInt32 CFM_PROTECTED	= 0x00000010;
		private const UInt32 CFM_LINK		= 0x00000020;
		private const UInt32 CFM_SIZE		= 0x80000000;
		private const UInt32 CFM_COLOR		= 0x40000000;
		private const UInt32 CFM_FACE		= 0x20000000;
		private const UInt32 CFM_OFFSET		= 0x10000000;
		private const UInt32 CFM_CHARSET	= 0x08000000;
		private const UInt32 CFM_SUBSCRIPT	= CFE_SUBSCRIPT | CFE_SUPERSCRIPT;
		private const UInt32 CFM_SUPERSCRIPT= CFM_SUBSCRIPT;

		private const byte CFU_UNDERLINENONE		= 0x00000000;
		private const byte CFU_UNDERLINE			= 0x00000001;
		private const byte CFU_UNDERLINEWORD		= 0x00000002; /* (*) displayed as ordinary underline	*/
		private const byte CFU_UNDERLINEDOUBLE		= 0x00000003; /* (*) displayed as ordinary underline	*/
		private const byte CFU_UNDERLINEDOTTED		= 0x00000004;
		private const byte CFU_UNDERLINEDASH		= 0x00000005;
		private const byte CFU_UNDERLINEDASHDOT		= 0x00000006;
		private const byte CFU_UNDERLINEDASHDOTDOT	= 0x00000007;
		private const byte CFU_UNDERLINEWAVE		= 0x00000008;
		private const byte CFU_UNDERLINETHICK		= 0x00000009;
		private const byte CFU_UNDERLINEHAIRLINE	= 0x0000000A; /* (*) displayed as ordinary underline	*/

		#endregion

		#endregion

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.SuspendLayout();
            // 
            // RichTextBoxEx
            // 
            this.DetectUrls = false;
            this.ResumeLayout(false);

		}
		#endregion

		#region Public Enums

		// Enum for possible RTF colors
		public enum RtfColor
		{
			Black, Maroon, Green, Olive, Navy, Purple, Teal, Gray, Silver,
			Red, Lime, Yellow, Blue, Fuchsia, Aqua, White, Empty
		}

		#endregion

		#region My Enums

		// Specifies the flags/options for the unmanaged call to the GDI+ method
		// Metafile.EmfToWmfBits().
		private enum EmfToWmfBitsFlags
		{

			// Use the default conversion
			EmfToWmfBitsFlagsDefault=0x00000000,

			// Embedded the source of the EMF metafiel within the resulting WMF
			// metafile
			EmfToWmfBitsFlagsEmbedEmf=0x00000001,

			// Place a 22-byte header in the resulting WMF file.  The header is
			// required for the metafile to be considered placeable.
			EmfToWmfBitsFlagsIncludePlaceable=0x00000002,

			// Don't simulate clipping by using the XOR operator.
			EmfToWmfBitsFlagsNoXORClip=0x00000004
		};

		#endregion

		#region My Structs

		// Definitions for colors in an RTF document
		private struct RtfColorDef
		{
			public const string Black = @"\red0\green0\blue0";
			public const string Maroon = @"\red128\green0\blue0";
			public const string Green = @"\red0\green128\blue0";
			public const string Olive = @"\red128\green128\blue0";
			public const string Navy = @"\red0\green0\blue128";
			public const string Purple = @"\red128\green0\blue128";
			public const string Teal = @"\red0\green128\blue128";
			public const string Gray = @"\red128\green128\blue128";
			public const string Silver = @"\red192\green192\blue192";
			public const string Red = @"\red255\green0\blue0";
			public const string Lime = @"\red0\green255\blue0";
			public const string Yellow = @"\red255\green255\blue0";
			public const string Blue = @"\red0\green0\blue255";
			public const string Fuchsia = @"\red255\green0\blue255";
			public const string Aqua = @"\red0\green255\blue255";
			public const string White = @"\red255\green255\blue255";
			public const string Empty = @"";
		}

		// Control words for RTF font families
		private struct RtfFontFamilyDef
		{
			public const string Unknown = @"\fnil";
			public const string Roman = @"\froman";
			public const string Swiss = @"\fswiss";
			public const string Modern = @"\fmodern";
			public const string Script = @"\fscript";
			public const string Decor = @"\fdecor";
			public const string Technical = @"\ftech";
			public const string BiDirect = @"\fbidi";
		}

		#endregion

		#region My Constants

		// Not used in this application.  Descriptions can be found with documentation
		// of Windows GDI function SetMapMode
		private const int MM_TEXT = 1;
		private const int MM_LOMETRIC = 2;
		private const int MM_HIMETRIC = 3;
		private const int MM_LOENGLISH = 4;
		private const int MM_HIENGLISH = 5;
		private const int MM_TWIPS = 6;

		// Ensures that the metafile maintains a 1:1 aspect ratio
		private const int MM_ISOTROPIC = 7;

		// Allows the x-coordinates and y-coordinates of the metafile to be adjusted
		// independently
		private const int MM_ANISOTROPIC = 8;

		// Represents an unknown font family
		private const string FF_UNKNOWN = "UNKNOWN";

		// The number of hundredths of millimeters (0.01 mm) in an inch
		// For more information, see GetImagePrefix() method.
		private const int HMM_PER_INCH = 2540;

		// The number of twips in an inch
		// For more information, see GetImagePrefix() method.
		private const int TWIPS_PER_INCH = 1440;

		#endregion

		#region My Privates

		// The default text color
		private RtfColor textColor;

		// The default text background color
		private RtfColor highlightColor;

		// Dictionary that maps color enums to RTF color codes
		private HybridDictionary rtfColor;

		// Dictionary that mapas Framework font families to RTF font families
		private HybridDictionary rtfFontFamily;

		// The horizontal resolution at which the control is being displayed
		private float xDpi;

		// The vertical resolution at which the control is being displayed
		private float yDpi;

		#endregion

		#region Elements required to create an RTF document

		/* RTF HEADER
		 * ----------
		 * 
		 * \rtf[N]		- For text to be considered to be RTF, it must be enclosed in this tag.
		 *				  rtf1 is used because the RichTextBox conforms to RTF Specification
		 *				  version 1.
		 * \ansi		- The character set.
		 * \ansicpg[N]	- Specifies that unicode characters might be embedded. ansicpg1252
		 *				  is the default used by Windows.
		 * \deff[N]		- The default font. \deff0 means the default font is the first font
		 *				  found.
		 * \deflang[N]	- The default language. \deflang1033 specifies US English.
		 * */
		public const string RTF_HEADER = @"{\rtf1\ansi\ansicpg1252\deff0\deflang1033";

		/* RTF DOCUMENT AREA
		 * -----------------
		 * 
		 * \viewkind[N]	- The type of view or zoom level.  \viewkind4 specifies normal view.
		 * \uc[N]		- The number of bytes corresponding to a Unicode character.
		 * \pard		- Resets to default paragraph properties
		 * \cf[N]		- Foreground color.  \cf1 refers to the color at index 1 in
		 *				  the color table
		 * \f[N]		- Font number. \f0 refers to the font at index 0 in the font
		 *				  table.
		 * \fs[N]		- Font size in half-points.
		 * */
		private const string RTF_DOCUMENT_PRE = @"\viewkind4\uc1\pard\cf1\f0\fs20";
		private const string RTF_DOCUMENT_POST = @"\cf0\fs17}";
		private string RTF_IMAGE_POST = @"}";

		#endregion

		#region Accessors

		// TODO: This can be ommitted along with RemoveBadCharacters
		// Overrides the default implementation of RTF.  This is done because the control
		// was originally developed to run in an instant messenger that uses the
		// Jabber XML-based protocol.  The framework would throw an exception when the
		// XML contained the null character, so I filtered out.
		public new string Rtf
		{
			get { return RemoveBadChars(base.Rtf); }
			set { base.Rtf = value; }
		}

		// The color of the text
		public RtfColor TextColor
		{
			get { return textColor; }
			set { textColor = value; }
		}

		// The color of the highlight
		public RtfColor HiglightColor
		{
			get { return highlightColor; }
			set { highlightColor = value; }
		}

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes the text colors, creates dictionaries for RTF colors and
		/// font families, and stores the horizontal and vertical resolution of
		/// the RichTextBox's graphics context.
		/// </summary>
		public RichTextBoxEx()
			: base()
		{

			InitializeComponent();
			// Initialize default text and background colors
			textColor = RtfColor.Black;
			highlightColor = RtfColor.Empty;

			// Initialize the dictionary mapping color codes to definitions
			rtfColor = new HybridDictionary();
			rtfColor.Add(RtfColor.Aqua, RtfColorDef.Aqua);
			rtfColor.Add(RtfColor.Black, RtfColorDef.Black);
			rtfColor.Add(RtfColor.Blue, RtfColorDef.Blue);
			rtfColor.Add(RtfColor.Fuchsia, RtfColorDef.Fuchsia);
			rtfColor.Add(RtfColor.Gray, RtfColorDef.Gray);
			rtfColor.Add(RtfColor.Green, RtfColorDef.Green);
			rtfColor.Add(RtfColor.Lime, RtfColorDef.Lime);
			rtfColor.Add(RtfColor.Maroon, RtfColorDef.Maroon);
			rtfColor.Add(RtfColor.Navy, RtfColorDef.Navy);
			rtfColor.Add(RtfColor.Olive, RtfColorDef.Olive);
			rtfColor.Add(RtfColor.Purple, RtfColorDef.Purple);
			rtfColor.Add(RtfColor.Red, RtfColorDef.Red);
			rtfColor.Add(RtfColor.Silver, RtfColorDef.Silver);
			rtfColor.Add(RtfColor.Teal, RtfColorDef.Teal);
			rtfColor.Add(RtfColor.White, RtfColorDef.White);
			rtfColor.Add(RtfColor.Yellow, RtfColorDef.Yellow);
			rtfColor.Add(RtfColor.Empty, RtfColorDef.Empty);

			// Initialize the dictionary mapping default Framework font families to
			// RTF font families
			rtfFontFamily = new HybridDictionary();
			rtfFontFamily.Add(FontFamily.GenericMonospace.Name, RtfFontFamilyDef.Modern);
			rtfFontFamily.Add(FontFamily.GenericSansSerif.Name, RtfFontFamilyDef.Swiss);
			rtfFontFamily.Add(FontFamily.GenericSerif.Name, RtfFontFamilyDef.Roman);
			rtfFontFamily.Add(FF_UNKNOWN, RtfFontFamilyDef.Unknown);

			// Get the horizontal and vertical resolutions at which the object is
			// being displayed
			using (Graphics _graphics = this.CreateGraphics())
			{
				xDpi = _graphics.DpiX;
				yDpi = _graphics.DpiY;
			}

		}

		/// <summary>
		/// Calls the default constructor then sets the text color.
		/// </summary>
		/// <param name="_textColor"></param>
		public RichTextBoxEx(RtfColor _textColor)
			: this()
		{
			InitializeComponent();
			textColor = _textColor;
		}

		/// <summary>
		/// Calls the default constructor then sets te text and highlight colors.
		/// </summary>
		/// <param name="_textColor"></param>
		/// <param name="_highlightColor"></param>
		public RichTextBoxEx(RtfColor _textColor, RtfColor _highlightColor)
			: this()
		{
			InitializeComponent();
			textColor = _textColor;
			highlightColor = _highlightColor;
		}

		#endregion

		#region Append RTF or Text to RichTextBox Contents

		/// <summary>
		/// Assumes the string passed as a paramter is valid RTF text and attempts
		/// to append it as RTF to the content of the control.
		/// </summary>
		/// <param name="_rtf"></param>
		public void AppendRtf(string _rtf)
		{

			// Move caret to the end of the text
			this.Select(this.TextLength, 0);

			// Since SelectedRtf is null, this will append the string to the
			// end of the existing RTF
			this.SelectedRtf = _rtf;
		}

		/// <summary>
		/// Assumes that the string passed as a parameter is valid RTF text and
		/// attempts to insert it as RTF into the content of the control.
		/// </summary>
		/// <remarks>
		/// NOTE: The text is inserted wherever the caret is at the time of the call,
		/// and if any text is selected, that text is replaced.
		/// </remarks>
		/// <param name="_rtf"></param>
		public void InsertRtf(string _rtf)
		{
			this.SelectedRtf = _rtf;
		}
		/// <summary>
		/// Appends the text using the current font, text, and highlight colors.
		/// </summary>
		/// <param name="_text"></param>
		public void AppendTextAsRtf(string _text)
		{
			AppendTextAsRtf(_text, this.Font);
		}


		/// <summary>
		/// Appends the text using the given font, and current text and highlight
		/// colors.
		/// </summary>
		/// <param name="_text"></param>
		/// <param name="_font"></param>
		public void AppendTextAsRtf(string _text, Font _font)
		{
			AppendTextAsRtf(_text, _font, textColor);
		}

		/// <summary>
		/// Appends the text using the given font and text color, and the current
		/// highlight color.
		/// </summary>
		/// <param name="_text"></param>
		/// <param name="_font"></param>
		/// <param name="_color"></param>
		public void AppendTextAsRtf(string _text, Font _font, RtfColor _textColor)
		{
			AppendTextAsRtf(_text, _font, _textColor, highlightColor);
		}

		/// <summary>
		/// Appends the text using the given font, text, and highlight colors.  Simply
		/// moves the caret to the end of the RichTextBox's text and makes a call to
		/// insert.
		/// </summary>
		/// <param name="_text"></param>
		/// <param name="_font"></param>
		/// <param name="_textColor"></param>
		/// <param name="_backColor"></param>
		public void AppendTextAsRtf(string _text, Font _font, RtfColor _textColor, RtfColor _backColor)
		{
			// Move carret to the end of the text
			this.Select(this.TextLength, 0);

			InsertTextAsRtf(_text, _font, _textColor, _backColor);
		}

		#endregion

		#region Insert Plain Text

		/// <summary>
		/// Inserts the text using the current font, text, and highlight colors.
		/// </summary>
		/// <param name="_text"></param>
		public void InsertTextAsRtf(string _text)
		{
			InsertTextAsRtf(_text, this.Font);
		}


		/// <summary>
		/// Inserts the text using the given font, and current text and highlight
		/// colors.
		/// </summary>
		/// <param name="_text"></param>
		/// <param name="_font"></param>
		public void InsertTextAsRtf(string _text, Font _font)
		{
			InsertTextAsRtf(_text, _font, textColor);
		}

		/// <summary>
		/// Inserts the text using the given font and text color, and the current
		/// highlight color.
		/// </summary>
		/// <param name="_text"></param>
		/// <param name="_font"></param>
		/// <param name="_color"></param>
		public void InsertTextAsRtf(string _text, Font _font, RtfColor _textColor)
		{
			InsertTextAsRtf(_text, _font, _textColor, highlightColor);
		}

		/// <summary>
		/// Inserts the text using the given font, text, and highlight colors.  The
		/// text is wrapped in RTF codes so that the specified formatting is kept.
		/// You can only assign valid RTF to the RichTextBox.Rtf property, else
		/// an exception is thrown.  The RTF string should follow this format ...
		/// 
		/// {\rtf1\ansi\ansicpg1252\deff0\deflang1033{\fonttbl{[FONTS]}{\colortbl ;[COLORS]}}
		/// \viewkind4\uc1\pard\cf1\f0\fs20 [DOCUMENT AREA] }
		/// 
		/// </summary>
		/// <remarks>
		/// NOTE: The text is inserted wherever the caret is at the time of the call,
		/// and if any text is selected, that text is replaced.
		/// </remarks>
		/// <param name="_text"></param>
		/// <param name="_font"></param>
		/// <param name="_color"></param>
		/// <param name="_color"></param>
		public void InsertTextAsRtf(string _text, Font _font, RtfColor _textColor, RtfColor _backColor)
		{

			StringBuilder _rtf = new StringBuilder();

			// Append the RTF header
			_rtf.Append(RTF_HEADER);

			// Create the font table from the font passed in and append it to the
			// RTF string
			_rtf.Append(GetFontTable(_font));

			// Create the color table from the colors passed in and append it to the
			// RTF string
			_rtf.Append(GetColorTable(_textColor, _backColor));

			// Create the document area from the text to be added as RTF and append
			// it to the RTF string.
			_rtf.Append(GetDocumentArea(_text, _font));

			this.SelectedRtf = _rtf.ToString();
		}

		/// <summary>
		/// Creates the Document Area of the RTF being inserted. The document area
		/// (in this case) consists of the text being added as RTF and all the
		/// formatting specified in the Font object passed in. This should have the
		/// form ...
		/// 
		/// \viewkind4\uc1\pard\cf1\f0\fs20 [DOCUMENT AREA] }
		///
		/// </summary>
		/// <param name="_text"></param>
		/// <param name="_font"></param>
		/// <returns>
		/// The document area as a string.
		/// </returns>
		private string GetDocumentArea(string _text, Font _font)
		{

			StringBuilder _doc = new StringBuilder();

			// Append the standard RTF document area control string
			_doc.Append(RTF_DOCUMENT_PRE);

			// Set the highlight color (the color behind the text) to the
			// third color in the color table.  See GetColorTable for more details.
			_doc.Append(@"\highlight2");

			// If the font is bold, attach corresponding tag
			if (_font.Bold)
				_doc.Append(@"\b");

			// If the font is italic, attach corresponding tag
			if (_font.Italic)
				_doc.Append(@"\i");

			// If the font is strikeout, attach corresponding tag
			if (_font.Strikeout)
				_doc.Append(@"\strike");

			// If the font is underlined, attach corresponding tag
			if (_font.Underline)
				_doc.Append(@"\ul");

			// Set the font to the first font in the font table.
			// See GetFontTable for more details.
			_doc.Append(@"\f0");

			// Set the size of the font.  In RTF, font size is measured in
			// half-points, so the font size is twice the value obtained from
			// Font.SizeInPoints
			_doc.Append(@"\fs");
			_doc.Append((int)Math.Round((2 * _font.SizeInPoints)));

			// Apppend a space before starting actual text (for clarity)
			_doc.Append(@" ");

			// Append actual text, however, replace newlines with RTF \par.
			// Any other special text should be handled here (e.g.) tabs, etc.
			_doc.Append(_text.Replace("\n", @"\par "));

			// RTF isn't strict when it comes to closing control words, but what the
			// heck ...

			// Remove the highlight
			_doc.Append(@"\highlight0");

			// If font is bold, close tag
			if (_font.Bold)
				_doc.Append(@"\b0");

			// If font is italic, close tag
			if (_font.Italic)
				_doc.Append(@"\i0");

			// If font is strikeout, close tag
			if (_font.Strikeout)
				_doc.Append(@"\strike0");

			// If font is underlined, cloes tag
			if (_font.Underline)
				_doc.Append(@"\ulnone");

			// Revert back to default font and size
			_doc.Append(@"\f0");
			_doc.Append(@"\fs20");

			// Close the document area control string
			_doc.Append(RTF_DOCUMENT_POST);

			return _doc.ToString();
		}

		#endregion

		#region Insert Image

		/// <summary>
		/// Inserts an image into the RichTextBox.  The image is wrapped in a Windows
		/// Format Metafile, because although Microsoft discourages the use of a WMF,
		/// the RichTextBox (and even MS Word), wraps an image in a WMF before inserting
		/// the image into a document.  The WMF is attached in HEX format (a string of
		/// HEX numbers).
		/// 
		/// The RTF Specification v1.6 says that you should be able to insert bitmaps,
		/// .jpegs, .gifs, .pngs, and Enhanced Metafiles (.emf) directly into an RTF
		/// document without the WMF wrapper. This works fine with MS Word,
		/// however, when you don't wrap images in a WMF, WordPad and
		/// RichTextBoxes simply ignore them.  Both use the riched20.dll or msfted.dll.
		/// </summary>
		/// <remarks>
		/// NOTE: The image is inserted wherever the caret is at the time of the call,
		/// and if any text is selected, that text is replaced.
		/// </remarks>
		/// <param name="_image"></param>
		public void InsertImage(Image _image)
		{

			StringBuilder _rtf = new StringBuilder();

			// Append the RTF header
			_rtf.Append(RTF_HEADER);

			// Create the font table using the RichTextBox's current font and append
			// it to the RTF string
			_rtf.Append(GetFontTable(this.Font));

			// Create the image control string and append it to the RTF string
			_rtf.Append(GetImagePrefix(_image));

			// Create the Windows Metafile and append its bytes in HEX format
			_rtf.Append(GetRtfImage(_image));

			// Close the RTF image control string
			_rtf.Append(RTF_IMAGE_POST);

			this.SelectedRtf = _rtf.ToString();
		}

		/// <summary>
		/// Creates the RTF control string that describes the image being inserted.
		/// This description (in this case) specifies that the image is an
		/// MM_ANISOTROPIC metafile, meaning that both X and Y axes can be scaled
		/// independently.  The control string also gives the images current dimensions,
		/// and its target dimensions, so if you want to control the size of the
		/// image being inserted, this would be the place to do it. The prefix should
		/// have the form ...
		/// 
		/// {\pict\wmetafile8\picw[A]\pich[B]\picwgoal[C]\pichgoal[D]
		/// 
		/// where ...
		/// 
		/// A	= current width of the metafile in hundredths of millimeters (0.01mm)
		///		= Image Width in Inches * Number of (0.01mm) per inch
		///		= (Image Width in Pixels / Graphics Context's Horizontal Resolution) * 2540
		///		= (Image Width in Pixels / Graphics.DpiX) * 2540
		/// 
		/// B	= current height of the metafile in hundredths of millimeters (0.01mm)
		///		= Image Height in Inches * Number of (0.01mm) per inch
		///		= (Image Height in Pixels / Graphics Context's Vertical Resolution) * 2540
		///		= (Image Height in Pixels / Graphics.DpiX) * 2540
		/// 
		/// C	= target width of the metafile in twips
		///		= Image Width in Inches * Number of twips per inch
		///		= (Image Width in Pixels / Graphics Context's Horizontal Resolution) * 1440
		///		= (Image Width in Pixels / Graphics.DpiX) * 1440
		/// 
		/// D	= target height of the metafile in twips
		///		= Image Height in Inches * Number of twips per inch
		///		= (Image Height in Pixels / Graphics Context's Horizontal Resolution) * 1440
		///		= (Image Height in Pixels / Graphics.DpiX) * 1440
		///	
		/// </summary>
		/// <remarks>
		/// The Graphics Context's resolution is simply the current resolution at which
		/// windows is being displayed.  Normally it's 96 dpi, but instead of assuming
		/// I just added the code.
		/// 
		/// According to Ken Howe at pbdr.com, "Twips are screen-independent units
		/// used to ensure that the placement and proportion of screen elements in
		/// your screen application are the same on all display systems."
		/// 
		/// Units Used
		/// ----------
		/// 1 Twip = 1/20 Point
		/// 1 Point = 1/72 Inch
		/// 1 Twip = 1/1440 Inch
		/// 
		/// 1 Inch = 2.54 cm
		/// 1 Inch = 25.4 mm
		/// 1 Inch = 2540 (0.01)mm
		/// </remarks>
		/// <param name="_image"></param>
		/// <returns></returns>
		private string GetImagePrefix(Image _image)
		{

			StringBuilder _rtf = new StringBuilder();

			// Calculate the current width of the image in (0.01)mm
			int picw = (int)Math.Round((_image.Width / xDpi) * HMM_PER_INCH);

			// Calculate the current height of the image in (0.01)mm
			int pich = (int)Math.Round((_image.Height / yDpi) * HMM_PER_INCH);

			// Calculate the target width of the image in twips
			int picwgoal = (int)Math.Round((_image.Width / xDpi) * TWIPS_PER_INCH);

			// Calculate the target height of the image in twips
			int pichgoal = (int)Math.Round((_image.Height / yDpi) * TWIPS_PER_INCH);

			// Append values to RTF string
			_rtf.Append(@"{\pict\wmetafile8");
			_rtf.Append(@"\picw");
			_rtf.Append(picw);
			_rtf.Append(@"\pich");
			_rtf.Append(pich);
			_rtf.Append(@"\picwgoal");
			_rtf.Append(picwgoal);
			_rtf.Append(@"\pichgoal");
			_rtf.Append(pichgoal);
			_rtf.Append(" ");

			return _rtf.ToString();
		}

		/// <summary>
		/// Use the EmfToWmfBits function in the GDI+ specification to convert a 
		/// Enhanced Metafile to a Windows Metafile
		/// </summary>
		/// <param name="_hEmf">
		/// A handle to the Enhanced Metafile to be converted
		/// </param>
		/// <param name="_bufferSize">
		/// The size of the buffer used to store the Windows Metafile bits returned
		/// </param>
		/// <param name="_buffer">
		/// An array of bytes used to hold the Windows Metafile bits returned
		/// </param>
		/// <param name="_mappingMode">
		/// The mapping mode of the image.  This control uses MM_ANISOTROPIC.
		/// </param>
		/// <param name="_flags">
		/// Flags used to specify the format of the Windows Metafile returned
		/// </param>
		[DllImportAttribute("gdiplus.dll")]
		private static extern uint GdipEmfToWmfBits(IntPtr _hEmf, uint _bufferSize,
			byte[] _buffer, int _mappingMode, EmfToWmfBitsFlags _flags);


		/// <summary>
		/// Wraps the image in an Enhanced Metafile by drawing the image onto the
		/// graphics context, then converts the Enhanced Metafile to a Windows
		/// Metafile, and finally appends the bits of the Windows Metafile in HEX
		/// to a string and returns the string.
		/// </summary>
		/// <param name="_image"></param>
		/// <returns>
		/// A string containing the bits of a Windows Metafile in HEX
		/// </returns>
		private string GetRtfImage(Image _image)
		{

			StringBuilder _rtf = null;

			// Used to store the enhanced metafile
			MemoryStream _stream = null;

			// Used to create the metafile and draw the image
			Graphics _graphics = null;

			// The enhanced metafile
			Metafile _metaFile = null;

			// Handle to the device context used to create the metafile
			IntPtr _hdc;

			try
			{
				_rtf = new StringBuilder();
				_stream = new MemoryStream();

				// Get a graphics context from the RichTextBox
				using (_graphics = this.CreateGraphics())
				{

					// Get the device context from the graphics context
					_hdc = _graphics.GetHdc();

					// Create a new Enhanced Metafile from the device context
					_metaFile = new Metafile(_stream, _hdc);

					// Release the device context
					_graphics.ReleaseHdc(_hdc);
				}

				// Get a graphics context from the Enhanced Metafile
				using (_graphics = Graphics.FromImage(_metaFile))
				{

					// Draw the image on the Enhanced Metafile
					_graphics.DrawImage(_image, new Rectangle(0, 0, _image.Width, _image.Height));

				}

				// Get the handle of the Enhanced Metafile
				IntPtr _hEmf = _metaFile.GetHenhmetafile();

				// A call to EmfToWmfBits with a null buffer return the size of the
				// buffer need to store the WMF bits.  Use this to get the buffer
				// size.
				uint _bufferSize = GdipEmfToWmfBits(_hEmf, 0, null, MM_ANISOTROPIC,
					EmfToWmfBitsFlags.EmfToWmfBitsFlagsDefault);

				// Create an array to hold the bits
				byte[] _buffer = new byte[_bufferSize];

				// A call to EmfToWmfBits with a valid buffer copies the bits into the
				// buffer an returns the number of bits in the WMF.  
				uint _convertedSize = GdipEmfToWmfBits(_hEmf, _bufferSize, _buffer, MM_ANISOTROPIC,
					EmfToWmfBitsFlags.EmfToWmfBitsFlagsDefault);

				// Append the bits to the RTF string
				for (int i = 0; i < _buffer.Length; ++i)
				{
					_rtf.Append(String.Format("{0:X2}", _buffer[i]));
				}

				return _rtf.ToString();
			}
			finally
			{
				if (_graphics != null)
					_graphics.Dispose();
				if (_metaFile != null)
					_metaFile.Dispose();
				if (_stream != null)
					_stream.Close();
			}
		}

		#endregion

		#region RTF Helpers

		/// <summary>
		/// Creates a font table from a font object.  When an Insert or Append 
		/// operation is performed a font is either specified or the default font
		/// is used.  In any case, on any Insert or Append, only one font is used,
		/// thus the font table will always contain a single font.  The font table
		/// should have the form ...
		/// 
		/// {\fonttbl{\f0\[FAMILY]\fcharset0 [FONT_NAME];}
		/// </summary>
		/// <param name="_font"></param>
		/// <returns></returns>
		private string GetFontTable(Font _font)
		{

			StringBuilder _fontTable = new StringBuilder();

			// Append table control string
			_fontTable.Append(@"{\fonttbl{\f0");

			//			for(int i = 0; i < rtfFontFamily.Count; i++)
			// If the font's family corresponds to an RTF family, append the
			// RTF family name, else, append the RTF for unknown font family.
			if (rtfFontFamily.Contains(_font.FontFamily.Name))
				_fontTable.Append(rtfFontFamily[_font.FontFamily.Name]);
			else
				_fontTable.Append(rtfFontFamily[FF_UNKNOWN]);

			// \fcharset specifies the character set of a font in the font table.
			// 0 is for ANSI.
			_fontTable.Append(@"\fcharset0 ");

			// Append the name of the font
			_fontTable.Append(_font.Name);

			// Close control string
			_fontTable.Append(@";}}");

			return _fontTable.ToString();
		}

		/// <summary>
		/// Creates a font table from the RtfColor structure.  When an Insert or Append
		/// operation is performed, _textColor and _backColor are either specified
		/// or the default is used.  In any case, on any Insert or Append, only three
		/// colors are used.  The default color of the RichTextBox (signified by a
		/// semicolon (;) without a definition), is always the first color (index 0) in
		/// the color table.  The second color is always the text color, and the third
		/// is always the highlight color (color behind the text).  The color table
		/// should have the form ...
		/// 
		/// {\colortbl ;[TEXT_COLOR];[HIGHLIGHT_COLOR];}
		/// 
		/// </summary>
		/// <param name="_textColor"></param>
		/// <param name="_backColor"></param>
		/// <returns></returns>
		private string GetColorTable(RtfColor _textColor, RtfColor _backColor)
		{

			StringBuilder _colorTable = new StringBuilder();

			// Append color table control string and default font (;)
			_colorTable.Append(@"{\colortbl ;");

			// Append the text color
			_colorTable.Append(rtfColor[_textColor]);
			_colorTable.Append(@";");

			// Append the highlight color
			_colorTable.Append(rtfColor[_backColor]);
			_colorTable.Append(@";}\n");

			return _colorTable.ToString();
		}

		/// <summary>
		/// Called by overrided RichTextBox.Rtf accessor.
		/// Removes the null character from the RTF.  This is residue from developing
		/// the control for a specific instant messaging protocol and can be ommitted.
		/// </summary>
		/// <param name="_originalRtf"></param>
		/// <returns>RTF without null character</returns>
		private string RemoveBadChars(string _originalRtf)
		{
			return _originalRtf.Replace("\0", "");
		}

		#endregion

		#region Printing Code
		//Convert the unit used by the .NET framework (1/100 inch)
		//and the unit used by Win32 API calls (twips 1/1440 inch)
		//		private const double anInch = 14.4;
		private const double anInch = 14.4;
		private const double panInch = 13.65;

		[StructLayout(LayoutKind.Sequential)]
		private struct RECT
		{
			public int Left;
			public int Top;
			public int Right;
			public int Bottom;
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct CHARRANGE
		{
			public int cpMin;         //First character of range (0 for start of doc)
			public int cpMax;           //Last character of range (-1 for end of doc)
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct FORMATRANGE
		{
			public IntPtr hdc;             //Actual DC to draw on
			public IntPtr hdcTarget;       //Target DC for determining text formatting
			public RECT rc;                //Region of the DC to draw to (in twips)
			public RECT rcPage;            //Region of the whole DC (page size) (in twips)
			public CHARRANGE chrg;         //Range of text to draw (see earlier declaration)
		}

		private const int EM_FORMATRANGE  = WM_USER + 57;


		// Render the contents of the RichTextBox for printing
		//	Return the last character printed + 1 (printing start from this point for next page)
		public int Print(int charFrom, int charTo, PrintPageEventArgs e)
		{
			int check = SizePrint(charFrom, charTo, e, 0.5F);
			return check;
		}
		public int SizePrint(int charFrom, int charTo, PrintPageEventArgs e, float size)
		{
			//Calculate the area to render and print
			float x = e.Graphics.DpiX;
			float y = e.Graphics.DpiY;
			RECT rectToPrint;
			rectToPrint.Top = (int)((double)e.MarginBounds.Top * anInch);
			rectToPrint.Bottom = (int)((double)e.MarginBounds.Bottom * anInch);
			rectToPrint.Left = (int)((double)e.MarginBounds.Left * anInch);
			rectToPrint.Right = (int)((double)e.MarginBounds.Right * anInch);

			//Calculate the size of the page
			RECT rectPage;
			rectPage.Top = (int)((double)e.PageBounds.Top * anInch);
			rectPage.Bottom = (int)((double)e.PageBounds.Bottom * anInch);
			rectPage.Left = (int)((double)e.PageBounds.Left * anInch);
			rectPage.Right = (int)((double)e.PageBounds.Right * anInch);

			IntPtr hdc = e.Graphics.GetHdc();

			FORMATRANGE fmtRange;
			fmtRange.chrg.cpMax = charTo;				//Indicate character from to character to
			fmtRange.chrg.cpMin = charFrom;
			fmtRange.hdc = hdc;                    //Use the same DC for measuring and rendering
			fmtRange.hdcTarget = hdc;              //Point at printer hDC
			fmtRange.rc = rectToPrint;             //Indicate the area on page to print
			fmtRange.rcPage = rectPage;            //Indicate size of page

			IntPtr res = IntPtr.Zero;

			IntPtr wparam = IntPtr.Zero;
			wparam = new IntPtr(1);

			//Get the pointer to the FORMATRANGE structure in memory
			IntPtr lparam= IntPtr.Zero;
			lparam = Marshal.AllocCoTaskMem(Marshal.SizeOf(fmtRange));
			Marshal.StructureToPtr(fmtRange, lparam, false);

			//Send the rendered data for printing
			res = SendMessage(Handle, EM_FORMATRANGE, wparam, lparam);

			//Free the block of memory allocated
			Marshal.FreeCoTaskMem(lparam);

			//Release the device context handle obtained by a previous call
			e.Graphics.ReleaseHdc(hdc);

			//Return last + 1 character printer
			return res.ToInt32();
		}
/***********************************************************************************************/
        public int Count(int length)
		{
            int check = 0;
            int pages = 0;
            int charFrom = 0;
            int charTo = length;
            while ( charFrom < length )
            {
			    check = PrintCount(charFrom, charTo, 0.5F);
                pages++;
                charFrom = check;
            }
			return pages;
		}
/***********************************************************************************************/
		public int PrintCount(int charFrom, int charTo, float size)
		{
			//Calculate the area to render and print
            //float x = e.Graphics.DpiX;
            //float y = e.Graphics.DpiY;
			float x = 600;
			float y = 600;
			RECT rectToPrint;
			rectToPrint.Top = (int)((double)100 * anInch);
			rectToPrint.Bottom = (int)((double)1100 * anInch);
			rectToPrint.Left = (int)((double)100 * anInch);
			rectToPrint.Right = (int)((double)750 * anInch);

			//Calculate the size of the page
			RECT rectPage;
			rectPage.Top = (int)((double)0 * anInch);
			rectPage.Bottom = (int)((double)1100 * anInch);
			rectPage.Left = (int)((double)0 * anInch);
			rectPage.Right = (int)((double)850 * anInch);

            PictureBox pb = new PictureBox();
            pb.SetBounds(0, 0, 850, 1100);
            Graphics g = pb.CreateGraphics();
            IntPtr hdc = g.GetHdc();

//            IntPtr hdc = e.Graphics.GetHdc();

			FORMATRANGE fmtRange;
			fmtRange.chrg.cpMax = charTo;				//Indicate character from to character to
			fmtRange.chrg.cpMin = charFrom;
            fmtRange.hdc = hdc;                    //Use the same DC for measuring and rendering
            fmtRange.hdcTarget = hdc;              //Point at printer hDC
			fmtRange.rc = rectToPrint;             //Indicate the area on page to print
			fmtRange.rcPage = rectPage;            //Indicate size of page

			IntPtr res = IntPtr.Zero;

			IntPtr wparam = IntPtr.Zero;
			wparam = new IntPtr(1);

			//Get the pointer to the FORMATRANGE structure in memory
			IntPtr lparam= IntPtr.Zero;
			lparam = Marshal.AllocCoTaskMem(Marshal.SizeOf(fmtRange));
			Marshal.StructureToPtr(fmtRange, lparam, false);

			//Send the rendered data for printing
			res = SendMessage(Handle, EM_FORMATRANGE, wparam, lparam);

			//Free the block of memory allocated
			Marshal.FreeCoTaskMem(lparam);

			//Release the device context handle obtained by a previous call
            g.ReleaseHdc(hdc);
//			e.Graphics.ReleaseHdc(hdc);

			//Return last + 1 character printer
			return res.ToInt32();
		}
/***********************************************************************************************/
		public int GetCursorPosition()
		{
			return 0;
		}
		#endregion

		#region Hyperlink Code
		/// <summary>
		/// Insert a given text as a link into the RichTextBox at the current insert position.
		/// </summary>
		/// <param name="text">Text to be inserted</param>
		public void InsertLink(string text)
		{
			InsertLink(text, this.SelectionStart);
		}

		/// <summary>
		/// Insert a given text at a given position as a link. 
		/// </summary>
		/// <param name="text">Text to be inserted</param>
		/// <param name="position">Insert position</param>
		public void InsertLink(string text, int position)
		{
			if (position < 0 || position > this.Text.Length)
				throw new ArgumentOutOfRangeException("position");

			this.SelectionStart = position;
			this.SelectedText = text;
			this.Select(position, text.Length);
			this.SetSelectionLink(true);
			this.Select(position + text.Length, 0);
		}

		/// <summary>
		/// Insert a given text at at the current input position as a link.
		/// The link text is followed by a hash (#) and the given hyperlink text, both of
		/// them invisible.
		/// When clicked on, the whole link text and hyperlink string are given in the
		/// LinkClickedEventArgs.
		/// </summary>
		/// <param name="text">Text to be inserted</param>
		/// <param name="hyperlink">Invisible hyperlink string to be inserted</param>
		public void InsertLink(string text, string hyperlink)
		{
			InsertLink(text, hyperlink, this.SelectionStart);
		}

		/// <summary>
		/// Insert a given text at a given position as a link. The link text is followed by
		/// a hash (#) and the given hyperlink text, both of them invisible.
		/// When clicked on, the whole link text and hyperlink string are given in the
		/// LinkClickedEventArgs.
		/// </summary>
		/// <param name="text">Text to be inserted</param>
		/// <param name="hyperlink">Invisible hyperlink string to be inserted</param>
		/// <param name="position">Insert position</param>
		public void InsertLink(string text, string hyperlink, int position)
		{
			if (position < 0 || position > this.Text.Length)
				throw new ArgumentOutOfRangeException("position");

			this.SelectionStart = position;
			this.SelectedRtf = @"{\rtf1\ansi "+text+@"\v #"+hyperlink+@"\v0}";
			this.Select(position, text.Length + hyperlink.Length + 1);
			this.SetSelectionLink(true);
			this.Select(position + text.Length + hyperlink.Length + 1, 0);
		}

		/// <summary>
		/// Set the current selection's link style
		/// </summary>
		/// <param name="link">true: set link style, false: clear link style</param>
		public void SetSelectionLink(bool link)
		{
			SetSelectionStyle(CFM_LINK, link ? CFE_LINK : 0);
		}
		/// <summary>
		/// Get the link style for the current selection
		/// </summary>
		/// <returns>0: link style not set, 1: link style set, -1: mixed</returns>
		public int GetSelectionLink()
		{
			return GetSelectionStyle(CFM_LINK, CFE_LINK);
		}


		private void SetSelectionStyle(UInt32 mask, UInt32 effect)
		{
			CHARFORMAT2_STRUCT cf = new CHARFORMAT2_STRUCT();
			cf.cbSize = (UInt32)Marshal.SizeOf(cf);
			cf.dwMask = mask;
			cf.dwEffects = effect;

			IntPtr wpar = new IntPtr(SCF_SELECTION);
			IntPtr lpar = Marshal.AllocCoTaskMem(Marshal.SizeOf(cf));
			Marshal.StructureToPtr(cf, lpar, false);

			IntPtr res = SendMessage(Handle, EM_SETCHARFORMAT, wpar, lpar);

			Marshal.FreeCoTaskMem(lpar);
		}

		private int GetSelectionStyle(UInt32 mask, UInt32 effect)
		{
			CHARFORMAT2_STRUCT cf = new CHARFORMAT2_STRUCT();
			cf.cbSize = (UInt32)Marshal.SizeOf(cf);
			cf.szFaceName = new char[32];

			IntPtr wpar = new IntPtr(SCF_SELECTION);
			IntPtr lpar = 	Marshal.AllocCoTaskMem(Marshal.SizeOf(cf));
			Marshal.StructureToPtr(cf, lpar, false);

			IntPtr res = SendMessage(Handle, EM_GETCHARFORMAT, wpar, lpar);

			cf = (CHARFORMAT2_STRUCT)Marshal.PtrToStructure(lpar, typeof(CHARFORMAT2_STRUCT));

			int state;
			// dwMask holds the information which properties are consistent throughout the selection:
			if ((cf.dwMask & mask) == mask)
			{
				if ((cf.dwEffects & effect) == effect)
					state = 1;
				else
					state = 0;
			}
			else
			{
				state = -1;
			}

			Marshal.FreeCoTaskMem(lpar);
			return state;
		}
		#endregion

		//Following code overrides the control + I key combination in order to allow for Ctrl + I (Italic) shortcut
		bool control=false; // temp variable for control key
		public override bool PreProcessMessage(ref Message msg)
		{
			if (msg.LParam.ToInt32()==1900545 && msg.WParam.ToInt32()==17)  // condition when control key is pressed
				control=true;
			if (msg.LParam.ToInt32()==-1071841279 && msg.WParam.ToInt32()==17) // condition when control key is Up
				control=false;
			if (msg.LParam.ToInt32()==1507329 && msg.WParam.ToInt32()==73 && control) // compare when control is pressed and with combination of i
			{
				// Here u raise your customize event
				return true;
			}
			return base.PreProcessMessage(ref msg);
		}

		protected override void OnDragEnter(DragEventArgs drgevent)
		{
			if (this.AllowDrop)
			{
				if (drgevent.Data.GetDataPresent(DataFormats.Text))
					drgevent.Effect = ddeLocal;
				else if (drgevent.Data.GetDataPresent(DataFormats.FileDrop))
					drgevent.Effect = DragDropEffects.All;
				else
					drgevent.Effect = DragDropEffects.None;
			}
			base.OnDragEnter(drgevent);
		}

		protected override void OnDragDrop(DragEventArgs drgevent)
		{
			EMRDragEventArgs emrDEA = new EMRDragEventArgs(drgevent, _localindex, ddeLocal);

			if (drgevent.Data.GetDataPresent(DataFormats.Text))
			{
				int start = this.SelectionStart;

				//				int pos = this.GetCharIndexFromPosition(new Point(emrDEA.X,emrDEA.Y));

				if (start >= emrDEA.SelectedIndex && start < emrDEA.SelectedIndex + seltext.Length)
				{
					base.OnDragDrop(drgevent);
					return;
				}

				if (emrDEA.EMRAllowedEffects == DragDropEffects.Move)
				{
					if (emrDEA.SelectedIndex >= 0)
					{
						this.Select(emrDEA.SelectedIndex, seltext.Length);
						this.SelectedText = "";
						//						this.Text = this.Text.Remove(emrDEA.SelectedIndex,textdata.Length);
						if (start > emrDEA.SelectedIndex)
							DDText(start-seltext.Length, selrtf);
						//							this.Text = this.Text.Insert(start-textdata.Length,textdata);
						else
							DDText(start, selrtf);
						//							this.Text = this.Text.Insert(start,textdata);
					}
					else
						DDText(start, selrtf);
					//						this.Text = this.Text.Insert(start,textdata);
				}
				else if (emrDEA.EMRAllowedEffects == DragDropEffects.Copy)
				{
					DDText(start, selrtf);
					//					this.Text = this.Text.Insert(start,textdata);
				}
				else if (emrDEA.EMRAllowedEffects == DragDropEffects.All)
				{
					DDText(start, selrtf);
					//					this.Text = this.Text.Insert(start,textdata);
				}
			}
			else if (drgevent.Data.GetDataPresent(DataFormats.FileDrop))
			{
				string[] filenames = (string[])drgevent.Data.GetData(DataFormats.FileDrop);
				if (filenames.Length > 0)
					OnFileDrop(emrDEA);
			}

			base.OnDragDrop(drgevent);
		}

		protected void DDText(int idx, string intext)
		{
			this.Select(idx, 0);
			this.SelectedRtf = intext;
		}

		public event EMRDragEventHandler FileDrop;

		protected void OnFileDrop(EMRDragEventArgs dea)
		{
			if ((FileDrop != null)&&(this.AllowDrop))
			{
				FileDrop(this, dea);
			}
		}

		//		public event EMRDragEventHandler TextDrop;
		//		
		//		protected void OnTextDrop(EMRDragEventArgs dea)
		//		{
		//			if((TextDrop != null)&&(this.AllowDrop))
		//			{
		//				TextDrop(this,dea);
		//			}
		//		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			int selstart = this.SelectionStart;
			int sellength = this.SelectionLength;

			if (sellength < 1)
				return;

			int pos = this.GetCharIndexFromPosition(new Point(e.X, e.Y));
			if (pos >= selstart && pos < selstart + sellength)
			{

				this.Cursor = System.Windows.Forms.Cursors.Arrow;
				_moving = true;
				_localindex = selstart;
			}
			else
				_localindex = -1;

			base.OnMouseDown(e);
		}

		protected override void OnMouseUp(MouseEventArgs e)
		{
			_moving = false;
			base.OnMouseUp(e);
		}

		private string seltext = "", selrtf = "";

		protected override void OnMouseMove(MouseEventArgs e)
		{
			if (e.Button == MouseButtons.None)
			{
				int selstart = this.SelectionStart;
				int sellength = this.SelectionLength;

				int pos = this.GetCharIndexFromPosition(new Point(e.X, e.Y));
				if (pos >= selstart && pos < selstart + sellength)
					this.Cursor = System.Windows.Forms.Cursors.Arrow;
				else
					this.Cursor = System.Windows.Forms.Cursors.IBeam;
			}
			else if ((e.Button == MouseButtons.Left)&&(_moving))
			{
				_moving = false;
				ddeLocal = DragDropEffects.Move;
				seltext = this.SelectedText;
				selrtf = this.SelectedRtf;
				this.DoDragDrop(this.SelectedRtf, DragDropEffects.Move);
			}
			base.OnMouseMove(e);
		}

		/// <summary>
		/// Override base LoadFile to process any header and footer information into document.
		/// </summary>
		/// <param name="path">Path to File</param>
		public new void LoadFile(string path)
		{
			try
			{
				if (File.Exists(path))
				{
					string newpath = ProcessRTF(path);

					base.LoadFile(newpath);
				}
			}
			catch (Exception ex)
			{
				G1.LogError("Error trying to process and load file \"" + path + "\"", ex, false);
			}
		}

		/// <summary>
		/// Override base LoadFile to process any header and footer information into document.
		/// </summary>
		/// <param name="path">Path to File</param>
		/// <param name="fileType">Type of file</param>
		public new void LoadFile(string path, RichTextBoxStreamType fileType)
		{
			try
			{
				if (File.Exists(path))
				{
					string newpath = ProcessRTF(path);

					base.LoadFile(newpath, fileType);
				}
			}
			catch (Exception ex)
			{
				G1.LogError("Error trying to process and load file \"" + path + "\"", ex, false);
			}
		}
		
		private const string HEADER = @"{\header \";
		private const string HEADERF = @"{\headerf \";
		private const string HEADERL = @"{\headerl \";
		private const string HEADERR = @"{\headerr \";

		private const string FOOTER = @"{\footer \";
		private const string FOOTERF = @"{\footerf \";
		private const string FOOTERL = @"{\footerl \";
		private const string FOOTERR = @"{\footerr \";

		private string _TemporaryFile;

		private class HeaderFooterDetail
		{
			public string Text;
			public int Index;
			public int Length;           

			public HeaderFooterDetail(string text, string format_code )
			{
				int endidx = 0, contains = 0;
				int idx = text.IndexOf(format_code);
				if (idx < 0)
					return;

				for (int i = idx + 1; i < text.Length; i++)
				{
					if (text[i] == '{')
						contains++;
					if (text[i] == '}')
					{
						if (contains > 0)
							contains--;
						else
						{
							endidx = i;
							break;
						}
					}
				}

				int length = endidx + 1 - idx;

				Length = length;
				Index = idx;
				Text = text.Substring(Index, Length);
			}

			public bool ContainsData
			{
				get
				{
					string temptxt = System.Text.RegularExpressions.Regex.Replace(Text, @"\\\S*|\{|\}", "");
					if (temptxt.Trim().Length > 0)
						return true;
					else
						return false;
				}
			}
		}

		/// <summary>
		/// Processes file for header and footer information. Includes it in the document so it is visible with a richtextbox
		/// </summary>
		/// <param name="filename">File to process</param>
		private string ProcessRTF(string filename)
		{
			StreamReader sr = new StreamReader(filename);
			string p = sr.ReadToEnd();
			sr.Close();
						
			_TemporaryFile = Path.GetTempFileName();

			Format_Header(ref p, new string[] { HEADERF, HEADER, HEADERR, HEADERL });
			Format_Footer(ref p, new string[] { FOOTERF, FOOTER, FOOTERR, FOOTERL });

			StreamWriter sw = new StreamWriter(_TemporaryFile);
			sw.Write(p);
			sw.Close();

			return _TemporaryFile;
		}

		/// <summary>
		/// Format document so headers will appear in richtext
		/// </summary>
		/// <param name="source_text">Source text to be formatted</param>
		/// <param name="formats">Formats to be searched for and modified accordingly</param>
		private void Format_Header(ref string source_text, string[] formats)
		{
			bool formatted = false;

			foreach (string format in formats)
			{
				int idx = source_text.IndexOf(format);
				if (idx < 0)
					continue;

				if (formatted)
					RemoveFormattingSection(ref source_text, format);
				else
				{
					HeaderFooterDetail _details = new HeaderFooterDetail(source_text, format);
					if (_details.ContainsData)
						formatted = ReplaceHeaderFormatting(ref source_text, format);
					else
						RemoveFormattingSection(ref source_text, format);
				}
			}
		}

		/// Replaced the specified formatting in the file so that it will show up in RichTextBox at the bottom of the file like a footer
		/// </summary>
		/// <param name="source_text">Source text where formatting is to be done</param>
		/// <param name="formatting">Formatting section to be formatted</param>
		/// <returns>Whether formatting was completed</returns>
		private void Format_Footer(ref string source_text, string[] formats)
		{
			bool formatted = false;

			foreach (string format in formats)
			{
				int idx = source_text.IndexOf(format);
				if (idx < 0)
					continue;

				if (formatted)
					RemoveFormattingSection(ref source_text, format);
				else
				{
					HeaderFooterDetail _details = new HeaderFooterDetail(source_text, format);
					if (_details.ContainsData)
						formatted = ReplaceFooterFormatting(ref source_text, format);
					else
						RemoveFormattingSection(ref source_text, format);
				}
			}
		}

		/// Replaced the specified formatting in the file so that it will show up in RichTextBox at the bottom of the file like a header
		/// </summary>
		/// <param name="source_text">Source text where formatting is to be done</param>
		/// <param name="formatting">Formatting section to be formatted</param>
		/// <returns>Whether formatting was completed</returns>
		private bool ReplaceHeaderFormatting(ref string source_text, string formatting)
		{
			if (source_text.IndexOf(formatting) >= 0)
			{
				source_text = source_text.Replace(formatting, @"{\");
				return true;
			}
			return false;
		}

		/// <summary>
		/// Replaced the specified formatting in the file so that it will show up in RichTextBox at the bottom of the file like a footer
		/// </summary>
		/// <param name="source_text">Source text where formatting is to be done</param>
		/// <param name="formatting">Formatting section to be formatted</param>
		/// <returns>Whether formatting was completed</returns>
		private bool ReplaceFooterFormatting(ref string source_text, string formatting)
		{
			if (source_text.IndexOf(formatting) >= 0)
			{
				int startIndex = source_text.IndexOf(formatting);
				int endIndex = GetEndIndex(startIndex, source_text);

				string foot = source_text.Substring(startIndex, endIndex + 1 - startIndex);
				foot = foot.Replace(formatting, @"{\");
				source_text = source_text.Remove(startIndex, endIndex + 1 - startIndex);
				source_text = source_text.Insert(source_text.Length - 1, foot);
				return true;
			}
			return false;
		}

		/// <summary>
		/// Removes the specified formatting section from the file
		/// </summary>
		/// <param name="source_text">Source text that formatting is to be removed from</param>
		/// <param name="formatting">Formatting section to be removed</param>
		/// <returns>Whether formatting was completed</returns>
		private bool RemoveFormattingSection(ref string text, string formatting)
		{
			int endidx = 0, contains = 0;
			int idx = text.IndexOf(formatting);
			if (idx < 0)
				return false;

			for (int i = idx + 1; i < text.Length; i++)
			{
				if (text[i] == '{')
					contains++;
				if (text[i] == '}')
				{
					if (contains > 0)
						contains--;
					else
					{
						endidx = i;
						break;
					}
				}
			}

			int length = endidx + 1 - idx;
			text = text.Remove(idx, length);
			return true;
		}

		/// <summary>
		/// Gets the ending index of {} group
		/// </summary>
		/// <param name="startidx">Beginning index</param>
		/// <param name="source_text">String to check</param>
		/// <returns>Ending index</returns>
		private int GetEndIndex(int startidx, string source_text)
		{
			int contains = 0;
			int endidx = 0;
			for (int i = startidx + 1; i < source_text.Length; i++)
			{
				if (source_text[i] == '{')
					contains++;
				if (source_text[i] == '}')
				{
					if (contains > 0)
						contains--;
					else
					{
						endidx = i;
						break;
					}
				}
			}
			return endidx;
		}

		/// <summary>
		/// Uses the "on disk" path to the rtf file to load the contents of the RichTextBoxEx.
		/// Appends the contents if the file contains "LETTER".
		/// </summary>
		/// <param name="rtfFileName">Full path to the FTP file to load</param>
		/// <returns>true on success : false on !File.Exists</returns>
		public bool getRTF_fromDB(string rtfFileName)
		{
			if (!File.Exists(rtfFileName))
				return false;
			if (rtfFileName.ToUpper().IndexOf("LETTER") > 0)
				return this.appendRTF_fromDB(rtfFileName);
			else
				return this.loadRTF_fromDB(rtfFileName);
		}

		/// <summary>
		/// Appends this with the rtf 
		/// </summary>
		/// <param name="rtfFileName"></param>
		/// <returns>true on complete</returns>
		private bool appendRTF_fromDB(string rtfFileName)
		{
			using (RichTextBoxEx richTextBoxEx_tempHolder = new RichTextBoxEx())
			{
				richTextBoxEx_tempHolder.LoadFile(rtfFileName);
				this.AppendRtf(richTextBoxEx_tempHolder.Rtf);
				richTextBoxEx_tempHolder.Visible = false;
				richTextBoxEx_tempHolder.Dispose();
			}
			return true;
		}

		/// <summary>
		/// Loads this with the rtf
		/// </summary>
		/// <param name="rtfFileName"></param>
		/// <returns>true on complete</returns>
		private bool loadRTF_fromDB(string rtfFileName)
		{
			this.Clear();
			return appendRTF_fromDB(rtfFileName);
		}



		public static void AddHeaderLine(RichTextBoxEx rtbPrint, string str, float newsize, bool bold, bool italics)
		{

			G1.Toggle_Bold(rtbPrint, bold, italics, (float)newsize);
			Color pen                               = rtbPrint.SelectionColor;
			rtbPrint.SelectionAlignment = HorizontalAlignment.Center;
			rtbPrint.AppendText(str+"\n");
			rtbPrint.SelectionAlignment = HorizontalAlignment.Left;
			rtbPrint.SelectionColor     = pen;
		}

		public Rectangle SelectionBounds
		{
			get
			{
				return GetSelectionBounds(this.SelectionStart, this.SelectionStart + this.SelectionLength);
			}
		}

		public Rectangle GetSelectionBounds(int start, int stop)
		{
			Point tl = GetPositionFromCharIndex(start);
			Point tr = GetPositionFromCharIndex(stop);
			int width = tr.X - tl.X;
			int height = Font.Height;
			return new Rectangle(tl, new Size(width, height));
		}
	}

	public class EMRDragEventArgs : DragEventArgs
	{
		//		DragEventArgs mDEA;
		//		public DragEventArgs DEA
		//		{
		//			get{return mDEA;}
		//		}

		private int mSelectedIndex = -1;
		public int SelectedIndex
		{
			get { return mSelectedIndex; }
			set { mSelectedIndex = value; }
		}

		private DragDropEffects mDDE = DragDropEffects.All;
		public DragDropEffects EMRAllowedEffects
		{
			get { return mDDE; }
			set { mDDE = value; }
		}

		public EMRDragEventArgs(DragEventArgs dea, int indx, DragDropEffects ddein)
			: base(dea.Data, dea.KeyState, dea.X, dea.Y, dea.AllowedEffect, dea.Effect)
		{
			mSelectedIndex = indx;
			mDDE = ddein;
		}
	}
}
