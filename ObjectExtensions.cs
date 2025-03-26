using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace GeneralLib
{
    public static class ObjectExtensions
    {
        /// <summary>
        /// Check that an object is Null or Blank
        /// </summary>
        /// <param name="val">Value to test</param>
        /// <returns>True if val is Null, DBNull, Blank or White Space, False if valid string</returns>
        public static DataTable ObjToDataTable<T>(this T[] students)
        {
            if (students == null || students.Length == 0) return null;

            DataTable table = new DataTable();
            var student_tmp = students[0];
            table.Columns.AddRange(student_tmp.GetType().GetFields().Select(field => new DataColumn(field.Name, field.FieldType)).ToArray());
            int fieldCount = student_tmp.GetType().GetFields().Count();

            students.All(student =>
            {
                table.Rows.Add(Enumerable.Range(0, fieldCount).Select(index => student.GetType().GetFields()[index].GetValue(student)).ToArray());
                return true;
            });

            return table;
        }

        public static bool ObjIsNullOrWhiteSpace(this object val)
        {
            return String.IsNullOrEmpty(val.ObjToString().Trim());
        }

        public static string Reverse(this string input)
        {
            return new string(input.ToCharArray().Reverse().ToArray());
        }
        /// <summary>
        /// Convert and return a Database value to a string or Blank if null
        /// </summary>
        /// <param name="val">Value to convert</param>
        /// <returns>Value if not null, Blank if null</returns>
        public static string ObjToString(this object val)
        {
            return (val != null && val != DBNull.Value) ? val.ToString() : String.Empty;
        }


            public static IEnumerable<string> SplitAndKeep(this string s, char[] delims)
            {
                int start = 0, index;
                while ((index = s.IndexOfAny(delims, start)) != -1)
                {
                    if (index - start > 0)
                        yield return s.Substring(start, index - start);
                    yield return s.Substring(index, 1);
                    start = index + 1;
                }
                if (start < s.Length)
                {
                    yield return s.Substring(start);
                }
            }
        /// <summary>
        /// Convert a Database value to a Integer value or 0 if null
        /// </summary>
        /// <param name="val">Value to check and return</param>
        /// <returns>Value or MinValue</returns>
        public static int ObjToInt32(this object val)
        {
            if (val == null)
                return 0;
            int returnValue = Int32.TryParse(val.ToString(), out returnValue) ? returnValue : 0;
            return returnValue;
        }

        /// <summary>
        /// Convert a Database value to a Integer value or 0 if null
        /// </summary>
        /// <param name="val">Value to check and return</param>
        /// <returns>Value or MinValue</returns>
        public static long ObjToInt64(this object val)
        {
            if (val == null)
                return 0;
            Int64 returnValue = Int64.TryParse(val.ToString(), out returnValue) ? returnValue : 0;
            return returnValue;
        }

        /// <summary>
        /// Convert a Database value to a double value or 0.00 if null
        /// </summary>
        /// <param name="val">Value to check and return</param>
        /// <returns>Value or MinValue</returns>
        public static double ObjToDouble(this object val)
        {
            if (val == null)
                return 0;
            double returnValue = Double.TryParse(val.ToString(), out returnValue) ? returnValue : 0;
            return returnValue;
        }
        public static decimal ObjToDecimal(this object val)
        {
            if (val == null)
                return 0;
            decimal returnValue = Decimal.TryParse(val.ToString(), out returnValue) ? returnValue : 0;
            return returnValue;
        }
        /// <summary>
        /// Convert a Database value to a double value or 0.00, then float if null
        /// </summary>
        /// <param name="val">Value to check and return</param>
        /// <returns>Value or MinValue</returns>
        public static float ObjToFloat(this object val)
        {
            if (val == null)
                return 0F;
            float returnValue = (float) (val.ObjToDouble());
            return returnValue;
        }

        public static byte[] ObjToBytes(this object obj)
        {
            if ((obj == null) || (obj == System.DBNull.Value))
                return null;

            return (byte[])obj;
        }
        /***********************************************************************************************/
        internal static DateTime UnixToDateTime(this object ticks )
        {
            // Unix timestamp is seconds past epoch
            try
            {
//                double unixTimeStamp = (double)ticks;
                System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
//                dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
                dtDateTime = dtDateTime.AddSeconds(ticks.ObjToDouble()).ToLocalTime();
                return dtDateTime;
            }
            catch
            {
                return DateTime.MinValue;
            }
        }
        /***********************************************************************************************/
        //internal static DateTime UnixToDateTime(this object val)
        //{
        //    try
        //    {
        //        System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc );
        //        dtDateTime = dtDateTime.AddSeconds(val.ObjToDouble()).ToLocalTime();
        //        return dtDateTime;
        //    }
        //    catch
        //    {
        //        return DateTime.MinValue;
        //    }
        //}
/***********************************************************************************************/
        public static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        /// <summary>
        /// Converts a DateTime to its Unix timestamp value. This is the number of seconds
        /// passed since the Unix Epoch (1/1/1970 UTC)
        /// </summary>
        /// <param name="aDate">DateTime to convert</param>
        /// <returns>Number of seconds passed since 1/1/1970 UTC </returns>
        public static int ToUnix(this DateTime aDate)
        {
            if (aDate == DateTime.MinValue)
            {
                return -1;
            }
            TimeSpan span = (aDate - UnixEpoch);
            return (int)Math.Floor(span.TotalSeconds);
        }
  
        /// <summary>
        /// Converts the specified 32 bit integer to a DateTime based on the number of seconds
        /// since the Unix epoch (1/1/1970 UTC)
        /// </summary>
        /// <param name="anInt">Integer value to convert</param>
        /// <returns>DateTime for the Unix int time value</returns>
        public static DateTime ToDateTime(this int anInt)
        {
            if (anInt == -1)
            {
                return DateTime.MinValue;
            }
            return UnixEpoch.AddSeconds(anInt);
        }
/***********************************************************************************************/
        public static bool ObjToBool(this object boolVal)
        {
            bool rtnval = false;
            if (boolVal != null && boolVal != DBNull.Value)
            {
                if (boolVal.ToString() == "1")
                    return true;
                else if (boolVal.ToString() == "0")
                    return false;
                else
                    Boolean.TryParse(boolVal.ToString(), out rtnval);
            }

            return rtnval;
        }

        public static bool ObjToPhpBool(this object boolVal)
		{
			if (boolVal.ObjIsNullOrWhiteSpace()) return false;

			if (boolVal.ObjToInt64() > 0) return true;

			if (boolVal.ObjToString() == "0") return false;

			bool testBool;
			return !Boolean.TryParse(boolVal.ToString(), out testBool) || testBool;
		}

        public static bool ObjToPHPBool(this object boolVal)
        {
            bool rtnval = false;
            if (boolVal != null && boolVal != DBNull.Value)
            {
                if (boolVal.ToString() == "1")
                    return true;
                else if (boolVal.ToString() == "0")
                    return false;
                else if (ObjToInt64(boolVal) > 0)
                    return true;
                else
                    Boolean.TryParse(boolVal.ToString(), out rtnval);
            }

            return rtnval;
        }
        public static string ValidateRtfFileName(this object fileNameVal)
        {
            return fileNameVal.ValidateFileNameExtension(".rtf");
        }

        public static string ValidateFileNameExtension(this object fileNameVal, string extension)
        {
            if (fileNameVal.ObjIsNullOrWhiteSpace())
                return "";

            extension = extension.StartsWith(".", StringComparison.CurrentCultureIgnoreCase)
                ? extension
                : "." + extension;
            var fileName = fileNameVal.ToString();
            return fileName.EndsWith(extension, StringComparison.CurrentCultureIgnoreCase) ? fileName : fileName + extension;
        }

        public static DateTime ObjToDateTime(this object dateTimeValue)
        {
            if (dateTimeValue == null || dateTimeValue == DBNull.Value)
                return DateTime.MinValue;

            var dt = DateTime.MinValue; 
            try
            {
                if (!DateTime.TryParse(dateTimeValue.ToString(), out dt))
                {
                    var ci = new System.Globalization.CultureInfo("en-US");
                    string[] expFormats = { "d", "g", "G", "t", "T", "yyyyMMdd", "MMddyyyy", "yyyyMMddHHmm", "yyyyMMddHHmmss", "MMddyyyyHHmmss", "yyyy-M-d", "yyyy/M/d", "yyyy/M/d HH:mm:ss", "yyyy-M-d HH:mm:ss", "yyyy-M-d HH:mm", "HH:mm", "HH:mm:ss", "MMddyy" };
                    DateTime.TryParseExact(dateTimeValue.ToString(), expFormats, ci,
                                           System.Globalization.DateTimeStyles.NoCurrentDateDefault, out dt);
                }
            }
            catch (Exception ex)
            {
//                G1.LogError(ex, false);
            }

            return dt;
        }
        public static bool ObjIsInt(this object val)
        {
            try
            {
                if (val == null) return false;
                long newval;
                return Int64.TryParse(val.ToString(), out newval);
            }
            catch
            {
                return false;
            }
        }

        public static bool Contains(this string source, string toCheck, StringComparison comp)
        {
            return source.IndexOf(toCheck, comp) >= 0;
        }
        /// <summary>
        /// Will take an object and return the boolean opposite
        /// If currentValue is NULL will return True (NULL == false)
        /// </summary>
        /// <param name="currentValue">Current Bool to Flip</param>
        /// <returns>The bool opposite of currentValue</returns>
        public static bool ToggleBool(this object currentValue)
        {
            bool currentBool = ObjToBool(currentValue);
            return currentBool ? false : true;
        }
    }
}
