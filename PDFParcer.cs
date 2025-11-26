using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using iTextSharp.text.pdf;
using System.Text.RegularExpressions;
using DevExpress.DataAccess.Native.Data;
using iTextSharp;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.xml;
using iTextSharp.text.pdf.parser;


using GeneralLib;

namespace GeneralLib
{
    /// <summary>
    /// Parses a PDF file and extracts the text from it.
    /// </summary>
    public class P1
    {
        /// BT = Beginning of a text object operator 
        /// ET = End of a text object operator
        /// Td move to the start of next line
        ///  5 Ts = superscript
        /// -5 Ts = subscript

        #region Fields

        #region _numberOfCharsToKeep
        /// <summary>
        /// The number of characters to keep, when extracting text.
        /// </summary>
        private static int _numberOfCharsToKeep = 15;
        #endregion

        #endregion

        public static bool IsPrintableAscii(byte value)
        {
            // Printable ASCII characters are typically considered to be in the range 32-126.
            return value >= 32 && value <= 126;
        }

        public static string GetxRef ( string  [] Lines )
        {
            string xRef = "";
            string str = "";
            bool gotxref = false;
            for ( int i=0; i<Lines.Length; i++)
            {
                str = Lines[i].ObjToString();
                if (str.IndexOf("trailer") == 0)
                    break;
                if ( gotxref )
                {
                    xRef += str + "\n";
                    continue;
                }
                if ( str.IndexOf ( "xref") == 0 )
                    gotxref = true;
                if (str.IndexOf("%%eof") == 0)
                    break;
            }
            return xRef;
        }
        #region ExtractText
        /// <summary>
        /// Extracts a text from a PDF file.
        /// </summary>
        /// <param name="inFileName">the full path to the pdf file.</param>
        /// <param name="outFileName">the output file name.</param>
        /// <returns>the extracted text</returns>
        public static string ExtractText(string inFileName, string outFileName)
        {
            string strText = "";


            StreamWriter outFile = null;
            try
            {
                StringBuilder sb = new StringBuilder();

                using (iTextSharp.text.pdf.PdfReader reader = new iTextSharp.text.pdf.PdfReader(inFileName))
                {
                    for ( int pageNo = 1; pageNo <= reader.NumberOfPages; pageNo++)
                    {
                        iTextSharp.text.pdf.parser.ITextExtractionStrategy its = new iTextSharp.text.pdf.parser.SimpleTextExtractionStrategy();
                        String s = iTextSharp.text.pdf.parser.PdfTextExtractor.GetTextFromPage(reader, pageNo, its);
                        s = Encoding.UTF8.GetString(ASCIIEncoding.Convert(Encoding.Default, Encoding.UTF8, Encoding.Default.GetBytes(s)));
                        sb.Append(s);
                    }
                }

                strText = sb.ToString();
                if (1 == 1)
                    return strText;

                byte[] fileBytes = File.ReadAllBytes(inFileName);

                string sstr = G1.ConvertToString(fileBytes);
                string xstr = string.Empty;
                //string xstr = G1.DecompressString(sstr);


                string[] Lines = sstr.Split('\n');
                string xRef = GetxRef(Lines);
                bool gotStream = false;
                string stream = "";
                for ( int i=0; i<Lines.Length; i++)
                {
                    sstr = Lines[i].ObjToString();
                    if ( sstr.IndexOf ( "stream") == 0 )
                    {
                        gotStream = true;
                        stream = string.Empty;
                        continue;
                    }
                    else if ( sstr.IndexOf ( "endstream") == 0 )
                    {
                        gotStream = false;
                        //sstr = G1.ConvertToString(Lines[i].ObjToBytes());
                        //xstr = G1.DecompressString(sstr);
                    }
                    else if ( gotStream )
                    {
                        stream += sstr;
                    }
                }

                //xstr = G1.decompress(fileBytes);


                //string stuff = xExtractTextFromPDFBytes(fileBytes);

                //string stuff = ExtractTextFromPDFBytes( fileBytes );
                char c;
                string myStr = " ";
                string  allString = "";
                string converted = "";
                for ( int i=0; i<fileBytes.Length; i++)
                {
                    if (IsPrintableAscii(fileBytes[i]))
                    {
                        converted = Encoding.UTF8.GetString(fileBytes, i, 1);
                        allString += converted;
                    }
                }

                string[] xLines = allString.Split('\n');


                ////PdfReader pdfReader = new PdfReader(inFileName);
                ////StringBuilder sb = new StringBuilder();
                ////byte[] bytes = pdfReader.AcroForm.GetBytes();
                ////foreach (var de in pdfReader.AcroFields.Fields)
                ////{
                ////    sb.Append(de.Key.ToString() + Environment.NewLine);
                ////}
                ////string str = sb.ToString();
                ////str = str.Replace("\r", "");
                ////string[] LLines = str.Split('\n');


                ////// Create a reader for the given PDF file
                //////                PdfReader reader = new PdfReader(inFileName);

                ////using (FileStream oFile = new FileStream( inFileName, FileMode.Create))
                ////{
                ////    //PdfReader pdfReader = new PdfReader(inFileName);


                ////    //StringBuilder sb = new StringBuilder();
                ////    //foreach (var de in pdfReader.AcroFields.Fields)
                ////    //{
                ////    //    sb.Append(de.Key.ToString() + Environment.NewLine);
                ////    //}
                ////    //string str = sb.ToString();
                ////    //str = str.Replace("\r", "");
                ////    //string[] Lines = str.Split('\n');

                ////    PdfStamper pdfStamper = new PdfStamper(pdfReader, oFile);
                ////    AcroFields fields = pdfStamper.AcroFields;
                ////    fields.SetField("First Name", "Robby");
                ////    str = fields.GetField("First Name");
                ////    pdfStamper.Close();
                ////    pdfReader.Close();
                ////}

                //    //                outFile = File.CreateText(outFileName);
                //    outFile = new StreamWriter(outFileName, false, System.Text.Encoding.UTF8);

                //    Console.Write("Processing: ");

                //    int totalLen = 68;
                //    float charUnit = ((float)totalLen) / (float)reader.NumberOfPages;
                //    int totalWritten = 0;
                //    float curUnit = 0;

                //    for (int page = 1; page <= reader.NumberOfPages; page++)
                //    {
                //        outFile.Write(ExtractTextFromPDFBytes(reader.GetPageContent(page)) + " ");

                //        str = ExtractTextFromPDFBytes(reader.GetPageContent(page)) + " ";
                //        pdfText += str;

                //    // Write the progress.
                //    if (charUnit >= 1.0f)
                //        {
                //            for (int i = 0; i < (int)charUnit; i++)
                //            {
                //                Console.Write("#");
                //                totalWritten++;
                //            }
                //        }
                //        else
                //        {
                //            curUnit += charUnit;
                //            if (curUnit >= 1.0f)
                //            {
                //                for (int i = 0; i < (int)curUnit; i++)
                //                {
                //                    Console.Write("#");
                //                    totalWritten++;
                //                }
                //                curUnit = 0;
                //            }

                //        }
                //    }

                //    if (totalWritten < totalLen)
                //    {
                //        for (int i = 0; i < (totalLen - totalWritten); i++)
                //        {
                //            Console.Write("#");
                //        }
                //    }
                //    return pdfText;
            }
            catch ( Exception ex)
            {
                return "";
            }
            finally
            {
                if (outFile != null) outFile.Close();
            }
            return strText;
        }
        #endregion

        #region ExtractTextFromPDFBytes
        /// <summary>
        /// This method processes an uncompressed Adobe (text) object 
        /// and extracts text.
        /// </summary>
        /// <param name="input">uncompressed</param>
        /// <returns></returns>
        public static string ExtractTextFromPDFBytes(byte[] input)
        {
            if (input == null || input.Length == 0) return "";

            string sstr = G1.ConvertToString(input);
            string[] Lines = sstr.Split('\n');
            for ( int i=0; i<Lines.Length; i++)
            {
                sstr = Lines[i];
                int idx = sstr.IndexOf("]TJ");
                if ( idx > 0 )
                {
                    string text = "";
                    for ( int j=0; j<sstr.Length; j++)
                    {
                        char c = (char)sstr[j];
                        if (c == '(' || c == ')' || c == '-' || (c >= '0' && c <= '9'))
                            continue;
                        text += sstr.Substring(j, 1);
                    }
                }
            }

            try
            {
                string resultString = "";

                // Flag showing if we are we currently inside a text object
                bool inTextObject = false;

                // Flag showing if the next character is literal 
                // e.g. '\\' to get a '\' character or '\(' to get '('
                bool nextLiteral = false;

                // () Bracket nesting level. Text appears inside ()
                int bracketDepth = 0;

                // Keep previous chars to get extract numbers etc.:
                char[] previousCharacters = new char[_numberOfCharsToKeep];
                for (int j = 0; j < _numberOfCharsToKeep; j++) 
                    previousCharacters[j] = ' ';


                for (int i = 0; i < input.Length; i++)
                {
                    char c = (char)input[i];
                    if (input[i] == 213)
                        c = "'".ToCharArray()[0];

                    if (inTextObject)
                    {
                        // Position the text
                        if (bracketDepth == 0)
                        {
                            if (CheckToken(new string[] { "TD", "Td" }, previousCharacters))
                            {
                                resultString += "\n\r";
                            }
                            else
                            {
                                if (CheckToken(new string[] { "'", "T*", "\"" }, previousCharacters))
                                {
                                    resultString += "\n";
                                }
                                else
                                {
                                    if (CheckToken(new string[] { "Tj" }, previousCharacters))
                                    {
                                        resultString += " ";
                                    }
                                }
                            }
                        }

                        // End of a text object, also go to a new line.
                        if (bracketDepth == 0 &&
                            CheckToken(new string[] { "ET" }, previousCharacters))
                        {

                            inTextObject = false;
                            resultString += " ";
                        }
                        else
                        {
                            // Start outputting text
                            if ((c == '(') && (bracketDepth == 0) && (!nextLiteral))
                            {
                                bracketDepth = 1;
                            }
                            else
                            {
                                // Stop outputting text
                                if ((c == ')') && (bracketDepth == 1) && (!nextLiteral))
                                {
                                    bracketDepth = 0;
                                }
                                else
                                {
                                    // Just a normal text character:
                                    if (bracketDepth == 1)
                                    {
                                        // Only print out next character no matter what. 
                                        // Do not interpret.
                                        if (c == '\\' && !nextLiteral)
                                        {
                                            resultString += c.ToString();
                                            nextLiteral = true;
                                        }
                                        else
                                        {
                                            if (((c >= ' ') && (c <= '~')) ||
                                                ((c >= 128) && (c < 255)))
                                            {
                                                resultString += c.ToString();
                                            }

                                            nextLiteral = false;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    // Store the recent characters for 
                    // when we have to go back for a checking
                    for (int j = 0; j < _numberOfCharsToKeep - 1; j++)
                    {
                        previousCharacters[j] = previousCharacters[j + 1];
                    }
                    previousCharacters[_numberOfCharsToKeep - 1] = c;

                    // Start of a text object
                    if (!inTextObject && CheckToken(new string[] { "BT" }, previousCharacters))
                    {
                        inTextObject = true;
                    }
                }

                return CleanupContent(resultString);
            }
            catch
            {
                return "";
            }
        }

        public static string CleanupContent(string text)
        {
            string[] patterns = { @"\\\(", @"\\\)", @"\\226", @"\\222", @"\\223", @"\\224", @"\\340", @"\\342", @"\\344", @"\\300", @"\\302", @"\\304", @"\\351", @"\\350", @"\\352", @"\\353", @"\\311", @"\\310", @"\\312", @"\\313", @"\\362", @"\\364", @"\\366", @"\\322", @"\\324", @"\\326", @"\\354", @"\\356", @"\\357", @"\\314", @"\\316", @"\\317", @"\\347", @"\\307", @"\\371", @"\\373", @"\\374", @"\\331", @"\\333", @"\\334", @"\\256", @"\\231", @"\\253", @"\\273", @"\\251", @"\\221" };
            string[] replace = { "(", ")", "-", "'", "\"", "\"", "à", "â", "ä", "À", "Â", "Ä", "é", "è", "ê", "ë", "É", "È", "Ê", "Ë", "ò", "ô", "ö", "Ò", "Ô", "Ö", "ì", "î", "ï", "Ì", "Î", "Ï", "ç", "Ç", "ù", "û", "ü", "Ù", "Û", "Ü", "®", "™", "«", "»", "©", "'" };

            for (int i = 0; i < patterns.Length; i++)
            {
                string regExPattern = patterns[i];
                Regex regex = new Regex(regExPattern, RegexOptions.IgnoreCase);
                text = regex.Replace(text, replace[i]);
            }

            return text;
        }

        #endregion

        #region CheckToken
        /// <summary>
        /// Check if a certain 2 character token just came along (e.g. BT)
        /// </summary>
        /// <param name="tokens">the searched token</param>
        /// <param name="recent">the recent character array</param>
        /// <returns></returns>
        public static bool CheckToken(string[] tokens, char[] recent)
        {
            foreach (string token in tokens)
            {
                if ((recent[_numberOfCharsToKeep - 3] == token[0]) &&
                    (recent[_numberOfCharsToKeep - 2] == token[1]) &&
                    ((recent[_numberOfCharsToKeep - 1] == ' ') ||
                    (recent[_numberOfCharsToKeep - 1] == 0x0d) ||
                    (recent[_numberOfCharsToKeep - 1] == 0x0a)) &&
                    ((recent[_numberOfCharsToKeep - 4] == ' ') ||
                    (recent[_numberOfCharsToKeep - 4] == 0x0d) ||
                    (recent[_numberOfCharsToKeep - 4] == 0x0a))
                    )
                {
                    return true;
                }
            }
            return false;
        }
        #endregion

        public static string xExtractTextFromPDFBytes(byte[] input)
        {
            if (input == null || input.Length == 0) return "";

            try
            {
                string resultString = "";

                // Flag showing if we are we currently inside a text object
                bool inTextObject = false;

                // Flag showing if the next character is literal
                // e.g. '\\' to get a '\' character or '\(' to get '('
                bool nextLiteral = false;

                // () Bracket nesting level. Text appears inside ()
                int bracketDepth = 0;

                // Keep previous chars to get extract numbers etc.:
                char[] previousCharacters = new char[_numberOfCharsToKeep];
                for (int j = 0; j < _numberOfCharsToKeep; j++) previousCharacters[j] = ' ';


                for (int i = 0; i < input.Length; i++)
                {
                    char c = (char)input[i];

                    if (inTextObject)
                    {
                        // Position the text
                        if (bracketDepth == 0)
                        {
                            if (CheckToken(new string[] { "TD", "Td" }, previousCharacters))
                            {
                                resultString += "\n\r";
                            }
                            else
                            {
                                if (CheckToken(new string[] { "'", "T*", "\"" }, previousCharacters))
                                {
                                    resultString += "\n";
                                }
                                else
                                {
                                    if (CheckToken(new string[] { "Tj" }, previousCharacters))
                                    {
                                        resultString += " ";
                                    }
                                }
                            }
                        }

                        // End of a text object, also go to a new line.
                        if (bracketDepth == 0 &&
                            CheckToken(new string[] { "ET" }, previousCharacters))
                        {

                            inTextObject = false;
                            resultString += " ";
                        }
                        else
                        {
                            // Start outputting text
                            if ((c == '(') && (bracketDepth == 0) && (!nextLiteral))
                            {
                                bracketDepth = 1;
                            }
                            else
                            {
                                // Stop outputting text
                                if ((c == ')') && (bracketDepth == 1) && (!nextLiteral))
                                {
                                    bracketDepth = 0;
                                }
                                else
                                {
                                    // Just a normal text character:
                                    if (bracketDepth == 1)
                                    {
                                        // Only print out next character no matter what.
                                        // Do not interpret.
                                        if (c == '\\' && !nextLiteral)
                                        {
                                            nextLiteral = true;
                                        }
                                        else
                                        {
                                            if (((c >= ' ') && (c <= '~')) ||
                                                ((c >= 128) && (c < 255)))
                                            {
                                                resultString += c.ToString();
                                            }

                                            nextLiteral = false;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    // Store the recent characters for
                    // when we have to go back for a checking
                    for (int j = 0; j < _numberOfCharsToKeep - 1; j++)
                    {
                        previousCharacters[j] = previousCharacters[j + 1];
                    }
                    previousCharacters[_numberOfCharsToKeep - 1] = c;

                    // Start of a text object
                    if (!inTextObject && CheckToken(new string[] { "BT" }, previousCharacters))
                    {
                        inTextObject = true;
                    }
                }
                return resultString;
            }
            catch
            {
                return "";
            }
        }


        //        public static string ReadPdfFile(object Filename, DataTable ReadLibray)
        public static string ReadPdfFile(object Filename)
        {
            PdfReader reader2 = null;
            try
            {
                reader2 = new PdfReader((string)Filename);
                //for (int i = 0; i < reader2.AcroForm.Fields.Count; i++)
                //{
                //    PRAcroForm.FieldInformation field = reader2.AcroForm.Fields[i];
                //    string name = field.Name.ObjToString();
                //    //                field.Info.Keys.
                //}
            }
            catch ( Exception ex)
            {

            }
            string strText = string.Empty;

            for (int page = 1; page <= reader2.NumberOfPages; page++)
            {
                ITextExtractionStrategy its = new iTextSharp.text.pdf.parser.SimpleTextExtractionStrategy();
                PdfReader reader = new PdfReader((string)Filename);
                String s = PdfTextExtractor.GetTextFromPage(reader, page, its);

                s = System.Text.Encoding.UTF8.GetString(System.Text.ASCIIEncoding.Convert(System.Text.Encoding.Default, Encoding.UTF8, Encoding.Default.GetBytes(s)));
                strText = strText + s;
                reader.Close();
            }
            return strText;
        }
    }
}