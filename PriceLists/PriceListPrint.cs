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
using DevExpress.XtraGrid.Views.Grid;
using GeneralLib;
using DevExpress.XtraPrinting;
using DevExpress.Utils;
using System.IO;
using DevExpress.XtraRichEdit;
using DevExpress.XtraRichEdit.API.Native;
using DevExpress.Office.Utils;

/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public partial class PriceListPrint : DevExpress.XtraEditors.XtraForm
    {
        private DataTable workDt = null;
        private DateTime workDateIn = DateTime.Now;
        private bool workUseDateIn = false;
        /***********************************************************************************************/
        public PriceListPrint( DataTable dt, DateTime dateIn, bool useDateIn )
        {
            InitializeComponent();
            workDt = dt;
            workDateIn = dateIn;
            workUseDateIn = useDateIn;
        }
        /***********************************************************************************************/
        private void PriceListPrint_Load(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            DataTable dt = workDt;

            if (G1.get_column_number(dt, "packageprice") < 0)
                dt.Columns.Add("packageprice", Type.GetType("System.Double"));

            DevExpress.XtraRichEdit.RichEditControl rtb1 = new RichEditControl();

            bool gotPrices = false;
            if (G1.get_column_number(dt, "price") >= 0)
                gotPrices = true;

//            AddFuneralHeading(rtb1);

            string str = "";
            byte[] bytes = null;
            MemoryStream stream = null;
            DevExpress.XtraRichEdit.RichEditControl rtb = new RichEditControl();

            bool underline = false;
            bool bold = false;
            float size = 9f;
            int indent = 0;
            int ipad = 0;
            string sValue = "";
            string desc = "";
            double pad = 0D;
            double dvalue = 0D;
            double bvalue = 0D;
            string holdingStr = "";
            double majorPad = 95;
            int holdCount = 0;
            int layin = 0;
            string saveDesc = "";
            string found = "";
            char c = (char)127;
            string baseString = " ";
            var sb = new StringBuilder(baseString);
            sb[0] = c;
            baseString = sb.ToString();

            DateTime date1 = workDateIn;

            int year = date1.Year;
            int day = date1.Day;
            string month = date1.ToString("MMMMMMMMMMMMM");

            string asOfDate = month + " " + day.ToString() + ", " + year.ToString("D4");

            bool includeBoth = false;

            DataTable localDt = new DataTable();
            localDt.Columns.Add("service");
            localDt.Columns.Add("packageprice");

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                try
                {
                    saveDesc = "";
                    underline = false;
                    bold = false;
                    size = 9f;
                    indent = 0;
                    layin = 0;
                    bytes = dt.Rows[i]["header"].ObjToBytes();
                    size = dt.Rows[i]["size"].ObjToFloat();
                    if (size <= 0f)
                        size = 9f;
                    str = dt.Rows[i]["underline"].ObjToString();
                    if (str.ToUpper().IndexOf("Y") >= 0)
                        underline = true;
                    str = dt.Rows[i]["bold"].ObjToString();
                    if (str.ToUpper().IndexOf("Y") >= 0)
                        bold = true;
                    indent = dt.Rows[i]["indent"].ObjToInt32();
                    if (indent <= 0)
                        indent = 0;
                    layin = dt.Rows[i]["layin"].ObjToInt32();
                    if (layin <= 0)
                        layin = 0;
                    found = dt.Rows[i]["found"].ObjToString();
                    if (bytes != null)
                    {
                        str = G1.ConvertToString(bytes);

                        if (!String.IsNullOrWhiteSpace(str))
                        {
                            stream = new MemoryStream(bytes);
                            rtb.Document.Delete(rtb.Document.Range);
                            rtb.Document.LoadDocument(stream, DevExpress.XtraRichEdit.DocumentFormat.Rtf);
                            str = rtb.Text.Trim();
                            if (workUseDateIn)
                            {
                                int idx = str.IndexOf("as of");
                                if (idx > 0)
                                {
                                    int xdx = str.IndexOf(" but");
                                    if (xdx > 0)
                                    {
                                        string text = str.Substring(idx, xdx - idx);
                                        rtb.RtfText = rtb.RtfText.Replace(text, "as of " + asOfDate);
                                        str = rtb.Text.Trim();
                                    }
                                }
                            }
                            if (!String.IsNullOrWhiteSpace(str))
                            {
                                DataRow dRow = localDt.NewRow();
                                dRow["service"] = rtb.Document.RtfText;
                                localDt.Rows.Add(dRow);
                                //rtb1.Document.AppendRtfText(rtb.Document.RtfText);
                            }
                        }
                    }
                    str = dt.Rows[i]["service"].ObjToString();
                    if (str == ".")
                        str = "";
                    if (!String.IsNullOrWhiteSpace(str))
                    {
                        if (str.Trim().ToUpper() == "{HEADER}")
                        {
                            //AddPageBreak(rtb1);
                            //AddFuneralHeading(rtb1);
                        }
                        else if (str.Trim().ToUpper() == "{EMPTY}")
                            str = baseString;
                        else if (str.Trim().ToUpper() == "{INCLUDE BOTH PRICES}")
                        {
                            includeBoth = true;
                        }
                        else if (str.Trim().ToUpper().IndexOf("{RANGE}") >= 0)
                        {
                            int idx = str.IndexOf('}');
                            str = str.Substring(idx + 1);
                            holdingStr = str.Trim();
                            pad = majorPad;
                            if (size != 9F)
                            {
                                pad = majorPad / size * 9D;
                                pad = pad - 1D;
                            }
                            pad = G1.RoundDown(pad);
                            pad = pad + indent;
                            ipad = Convert.ToInt32(pad);

                            holdingStr = holdingStr.PadRight(ipad);
                        }
                        else
                        {
                            dvalue = 3095D;
                            sValue = "$" + G1.ReformatMoney(dvalue);
                            if (gotPrices)
                            {
                                desc = dt.Rows[i]["price"].ObjToString();
                                if (!String.IsNullOrWhiteSpace(desc))
                                {
                                    dvalue = dt.Rows[i]["price"].ObjToDouble();
                                    sValue = "$" + G1.ReformatMoney(dvalue);
                                    if (includeBoth)
                                    {
                                        bvalue = dt.Rows[i]["packageprice"].ObjToDouble();
                                        sValue += "     $" + G1.ReformatMoney(bvalue);
                                    }
                                }
                                else
                                    sValue = "";
                            }
                            else
                            {
                                if (includeBoth)
                                {
                                    //sValue += "     $" + G1.ReformatMoney(dvalue);
                                }
                            }
                            if (found != "Y")
                            {
                                if (sValue == "$0.00")
                                    sValue = baseString;
                                else if (sValue == "$0.00     $0.00")
                                    sValue = baseString;
                            }

                            desc = "";
                            pad = majorPad;
                            if (size != 9F)
                            {
                                pad = majorPad / size * 9D;
                                pad = pad - 1D;
                            }
                            pad = G1.RoundDown(pad);
                            pad = pad + indent;
                            ipad = Convert.ToInt32(pad);

                            desc = desc.PadRight(ipad);
                            if (!String.IsNullOrWhiteSpace(holdingStr))
                            {
                                if (holdCount == 0)
                                {
                                    holdingStr = holdingStr.TrimEnd();
                                    holdingStr += "          " + sValue + "   to   ";
                                    holdCount++;
                                }
                                else
                                {
                                    desc = "";
                                    holdingStr += sValue;
                                    if (indent > 0)
                                        desc = " ".PadRight(indent);
                                    desc += holdingStr;
                                    DataRow dRow = localDt.NewRow();
                                    dRow["service"] = desc;
                                    localDt.Rows.Add();
                                    //AddNormalLine(rtb1, desc + "\n", bold, size, "Lucida Console", ParagraphAlignment.Left, underline);
                                    holdCount = 0;
                                    holdingStr = "";
                                }
                            }
                            else
                            {
                                if (layin <= 0)
                                {
                                    desc = G1.lay_in_string(desc, str, indent, str.Length);
                                    int ll = desc.Length;
                                    //sValue += "~";
                                    desc = G1.lay_in_string(desc, sValue, desc.Length - (sValue.Length + indent), sValue.Length);
                                    ll = desc.Length;
                                    if (desc.IndexOf("{") < 0 && desc.IndexOf("}") < 0)
                                    {
                                        DataRow dRow = localDt.NewRow();
                                        dRow["service"] = desc;
                                        localDt.Rows.Add();
                                        //AddNormalLine(rtb1, desc + "\n", bold, size, "Lucida Console", ParagraphAlignment.Left, underline);
                                        //AddParagraphMark(rtb1);
                                    }
                                }
                                else
                                    saveDesc = str;
                                G1.Toggle_Bold(rtb1, false, false);
                            }
                        }
                    }
                    //AddNormalText(rtb1, str + "\n", false, 12f);
                    //rtb1.Document.AppendText(str + "\n");

                    bytes = dt.Rows[i]["tail"].ObjToBytes();
                    if (bytes != null)
                    {
                        str = G1.ConvertToString(bytes);

                        if (!String.IsNullOrWhiteSpace(str))
                        {
                            stream = new MemoryStream(bytes);
                            rtb.Document.Delete(rtb.Document.Range);
                            rtb.Document.LoadDocument(stream, DevExpress.XtraRichEdit.DocumentFormat.Rtf);

                            str = rtb.Text;
                            if (!String.IsNullOrWhiteSpace(str))
                            {
                                DataRow dRow = localDt.NewRow();
                                dRow["service"] = rtb.Document.RtfText;
                                localDt.Rows.Add(dRow);
                                if (layin > 0)
                                {
                                    pad = majorPad;
                                    if (size != 9F)
                                    {
                                        pad = majorPad / size * 9D;
                                        pad = pad - 1D;
                                    }
                                    pad = G1.RoundDown(pad);
                                    pad = pad + indent;
                                    ipad = Convert.ToInt32(pad);
                                    //desc = G1.lay_in_string(saveDesc, str, layin, str.Length);
                                    desc = desc.PadRight(layin);
                                    //SetFont(rtb1, "Lucida Console", size, bold, false);
                                    saveDesc = saveDesc.PadRight(layin);
                                    dRow = localDt.NewRow();
                                    dRow["service"] = saveDesc;
                                    localDt.Rows.Add(dRow);
                                    //AddNormal(rtb1, saveDesc, bold, size, "Lucida Console", ParagraphAlignment.Left, underline);

                                    //rtb1.Document.AppendText(saveDesc);
                                    //SetFont(rtb1, "Lucida Console", size, false, false);

                                    dRow = localDt.NewRow();
                                    dRow["service"] = str;
                                    localDt.Rows.Add(dRow);
                                    // AddNormal(rtb1, str, false, size, "Lucida Console", ParagraphAlignment.Left, underline);
                                    //rtb1.Document.AppendText(str);
                                    sValue = sValue.PadLeft(ipad - layin - str.Length);
                                    //                                SetFont(rtb1, "Lucida Console", size, bold, false);
                                    dRow = localDt.NewRow();
                                    dRow["packageprice"] = sValue;
                                    localDt.Rows.Add(dRow);
                                    //AddNormal(rtb1, sValue + "\n", bold, size, "Lucida Console", ParagraphAlignment.Left, underline);
                                    //rtb1.Document.AppendText("\n");
                                    //rtb1.Document.AppendText(sValue + "\n");
                                    //AddNormalText(rtb1, desc, bold, size);
                                    //str = str.PadRight(ipad - layin);
                                    //SetFont(rtb1, "Lucida Console", size, false, false);
                                    //AddNormalText(rtb1, str, false, size);
                                    //SetFont(rtb1, "Lucida Console", size, bold, false);
                                    //AddNormalText(rtb1, sValue + "\n", bold, size);
                                    //AddNormalLine(rtb1, desc + "\n", bold, size, "Lucida Console", ParagraphAlignment.Left, underline);
                                }
                                else
                                {
                                    //AddParagraphMark(rtb);
                                    dRow = localDt.NewRow();
                                    dRow["packageprice"] = sValue;
                                    localDt.Rows.Add(dRow);
                                    //rtb1.Document.AppendRtfText(rtb.Document.RtfText);
                                }
                            }
                            else if (str == "\r\n")
                            {
                                DataRow dRow = localDt.NewRow();
                                localDt.Rows.Add(dRow);
                                //rtb1.Document.AppendText("\n");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {

                }
            }

            //str = rtb1.Document.RtfText;

            dgv.DataSource = localDt;
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void gridMain_CalcRowHeight(object sender, DevExpress.XtraGrid.Views.Grid.RowHeightEventArgs e)
        {
            GridView view = sender as GridView;
            if (view == null)
                return;
            if (e.RowHandle >= 0)
            {
                int height = (int)view.GetDataRow(e.RowHandle)["RowHeight"].ObjToInt32();
                if (height < 1)
                    height = 1;
                if (height >= 1)
                {
                    height = 15 * height;
                }
                e.RowHeight = height;
            }
        }
        /***********************************************************************************************/
    }
}