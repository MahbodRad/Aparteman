using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.Linq;
using System.ComponentModel;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Collections.Concurrent;
using NPOI.SS.UserModel;
using Aparteman.Models;
using System.Globalization;

namespace Aparteman.Services
{
    public static class Center
    {
        // کلیه دستوراتی که تغییراتی در دیتابیس میدهد لازم است که دستور ذخیره شود
        // متدهایی که با LogAction همراه هستند، دستور کاربر را ذخیره میکنند

        public static string PdfName(Int32 ID = 0)
        {
            return $"Chap_{ID}.Pdf";
        }
        public static void ChapEmpty(string webRootPath, string Chap, string ext)
        {
            string filePath = Path.Combine(webRootPath, "Uploads");
            File.Copy(Path.Combine(filePath, "Empty" + ext), Path.Combine(filePath, Chap + ext), true);
        }

        public static string ExcelName(Int32 ID = 0)
        {
            return $"Chap_{ID}.xlsx";
        }
        public static void DeleteFileIfExists(string FilePath)
        {
            var file = new FileInfo(FilePath);
            if (file.Exists)
                file.Delete();
        }
        public static void CellAddData(IRow row, int col, string Value, CellType Type)
        {

            // ایجاد سلول در این سطر
            ICell cell = row.CreateCell(col, Type);
            // نوشتن در سلول
            if (Type == CellType.Numeric)
                try
                {
                    cell.SetCellValue(Convert.ToDouble(Value));
                }
                catch
                {
                    cell.SetCellValue(Value);
                }
            else
                cell.SetCellValue(Value);
        }
        public static void CellAddDataNew(IRow row, int col, string Value, CellType Type, ICellStyle Style)
        {

            // ایجاد سلول در این سطر
            ICell cell = row.CreateCell(col, Type);
            cell.CellStyle = Style;
            // نوشتن در سلول
            if (Type == CellType.Numeric)
                try
                {
                    cell.SetCellValue(Convert.ToDouble(Value));
                }
                catch
                {
                    cell.SetCellValue(Value);
                }
            else
                cell.SetCellValue(Value);
        }


        public static string CommandText(string procName, Dictionary<string, object> parameters = null)
        { // متن دستور SQL برای اجرا
            //تبدیل پارامترها به جیسون
            string Params = JsonConvert.SerializeObject(parameters);
            // اصلاحات برای ذخیره مناسب
            Params = Params.Replace("\"@", "@").Replace("\":", "=");
            Params = Params.Replace("{", "").Replace("}", "").Replace("\"", "'");
            return $"Exec {procName} {Params} ";
        }


        public static string MonthName(string MN)
        {
            string MNAME = MN.Replace("BB", "بهار").Replace("TT", "تابستان").Replace("PP", "پائیز").Replace("ZZ", "زمستان").Replace("01", "فروردین").Replace("02", "اردیبهشت").Replace("03", "خرداد").Replace("04", "تیر").Replace("05", "مرداد").Replace("06", "شهریور").Replace("07", "مهر").Replace("08", "آبان").Replace("09", "آذر").Replace("10", "دی").Replace("11", "بهمن").Replace("12", "اسفند");
            return MNAME;
        }
        public static string MonthNameFull(string MN)
        {
            string MNAME = MN.Replace("/BB", "/بهار").Replace("/TT", "/تابستان").Replace("/PP", "/پائیز").Replace("/ZZ", "/زمستان").Replace("/01", "/فروردین").Replace("/02", "/اردیبهشت").Replace("/03", "/خرداد").Replace("/04", "/تیر").Replace("/05", "/مرداد").Replace("/06", "/شهریور").Replace("/07", "/مهر").Replace("/08", "/آبان").Replace("/09", "/آذر").Replace("/10", "/دی").Replace("/11", "/بهمن").Replace("/12", "/اسفند");
            return MNAME;
        }
        
        public static void GetUserData(string userCooki, UserInfo userInfo)
        { // بازگزداندن دیتای کاربر از روی کوکی
            var legacyCookie = userCooki.FromLegacyCookieString();
            if (legacyCookie != null)
            {
                userInfo.Knd = legacyCookie["Knd"];
                userInfo.Id = int.Parse(legacyCookie["Si"]);
            }
        }
        public static Int32 GetUserId(string userCooki)
        { // بازگرداندن آیدی کاربر از روی کوکی
            var legacyCookie = userCooki.FromLegacyCookieString();
            Int32 UserSi = 0;
            if (legacyCookie != null)
                UserSi = int.Parse(legacyCookie["Si"]);
            return UserSi;
        }
        public static string GetUserName(string userCooki)
        { // بازگرداندن آیدی کاربر از روی کوکی
            var legacyCookie = userCooki.FromLegacyCookieString();
            string UserName = "0";
            if (legacyCookie != null)
                UserName = legacyCookie["UserName"];
            return UserName;
        }

        public static string CommaClear(object NUMBER)
        {
            if (NUMBER == null)
                return "0";

            string number = NUMBER.ToString().Replace(" ", "").Replace(",", "");
            if (number == "")
                return "0";
            else
            {
                number = EnglishNumber(number);
                return number;
            }
        }
        public static string TrimTxt(string text)
        {
            if (text == "" || text == null)
                return "";
            else
                return text.Trim().Replace("'", "^");
        }
        public static string EnglishNumber(string NUMBER = "1234567890")
        {
            if (NUMBER == null)
                NUMBER = "";
            return NUMBER.Replace("۰", "0").Replace("۱", "1").Replace("۲", "2").Replace("۳", "3").Replace("۴", "4").Replace("۵", "5").Replace("۶", "6").Replace("۷", "7").Replace("۸", "8").Replace("۹", "9");
        }


        public static string Encrypt(string clearText)
        {
            string EncryptionKey = "Aparteman";
            byte[] clearBytes = Encoding.Unicode.GetBytes(clearText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }
                    clearText = Convert.ToBase64String(ms.ToArray());
                }
            }
            return clearText;
        }
        public static string Decrypt(string cipherText)
        {
            string EncryptionKey = "Aparteman";
            cipherText = cipherText.Replace(" ", "+");
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                        cs.Close();
                    }
                    cipherText = Encoding.Unicode.GetString(ms.ToArray());
                }
            }
            return cipherText;
        }

        public static string ToComma(this object value, int decimals = 0, string nullText = "0")
        {
            if (value == null || value == DBNull.Value)
                return nullText;

            if (decimal.TryParse(value.ToString(), out decimal result))
                return result.ToString($"N{decimals}");

            return nullText;
        }


        public static string ToMony(this object value, string nullText = "0")
        {
            if (value == null || value == DBNull.Value)
                return nullText;

            if (!decimal.TryParse(value.ToString(), out decimal result))
                return nullText;

            var culture = new CultureInfo("fa-IR");

            //culture.NumberFormat.CurrencySymbol = "ریال";
            culture.NumberFormat.CurrencyPositivePattern = 3;
            culture.NumberFormat.CurrencyNegativePattern = 8;

            return result.ToString("N0", culture); // بدون اعشار
        }

        public static string ToMonySymbol(this object value, string nullText = "0")
        {
            if (value == null || value == DBNull.Value)
                return nullText;

            if (!decimal.TryParse(value.ToString(), out decimal result))
                return nullText;

            var culture = new CultureInfo("fa-IR");
            culture.NumberFormat.CurrencyPositivePattern = 3;
            culture.NumberFormat.CurrencyNegativePattern = 8;

            // فقط عدد، بدون CurrencySymbol
            string number = result.ToString("N0", culture);

            // سمبل داخل span برای استایل‌دهی
            if (result < 0)
            {
                return $"<span class='text-danger'>{number}</span> <span class='rial-symbol icofont-riyal'></span>";
            }
            else
            {
                return $"{number} <span class='rial-symbol icofont-riyal'></span>";
            }
        }

        public static string ToCommaAshar(this object value, int decimals = 0, string nullText = "0")
        {
            if (value == null || value == DBNull.Value)
                return nullText;

            if (!decimal.TryParse(value.ToString(), out decimal result))
                return nullText;

            string formatted = result.ToString($"N{decimals}"); // 123,456.78
            int dotIndex = formatted.LastIndexOf('.');
            int Ashar = dotIndex + 1;

            string intPart = dotIndex > -1 ? formatted[..dotIndex] : formatted;

            string decPart = dotIndex > -1 ? formatted[Ashar..] : "";
            decPart = (int.Parse(decPart) == 0) ? "" : "." + decPart;
            // سمبل داخل span برای استایل‌دهی
            if (result < 0)
            {
                return $"<span class='int text-danger'>{intPart}</span><span class='ashar'>{decPart}</span>";
            }
            else
            {
                return $"<span class='int'>{intPart}</span><span class='ashar'>{decPart}</span>";
            }
        }

        public static string ToYesNo(this object value, string nullText = "0")
        { // تبدیل مقدار بولین true false به 1 یا 0
            if (value == null || value == DBNull.Value)
                return nullText;

            if(Convert.ToBoolean(value))
                return "1"; 
            else
                return "0";

        }

    }

}
