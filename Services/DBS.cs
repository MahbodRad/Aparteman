using Aparteman.Models;
using Azure;
using Azure.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using NPOI.SS.Formula.Functions;
using System.Collections.Concurrent;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace Aparteman.Services
{
    public class DBS
    {
        private static readonly string _ApartemanConnection;

        static DBS()
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddJsonFile("appsettings.Production.json", optional: true);

            IConfiguration config = builder.Build();
            _ApartemanConnection = config.GetConnectionString("DbsAparteman");
        }
        private static SqlConnection GetApartemaConnection()
        {
            return new SqlConnection(_ApartemanConnection);
        }

        private static readonly ConcurrentDictionary<string, SqlParameter[]> _cache
                                = new ConcurrentDictionary<string, SqlParameter[]>();
        public static SqlParameter[] GetCachedParameters(SqlConnection conn, string storedProcedureName)
        {  // کش کردن پارامترهای پروسجورها برای تشخیص نوع و سایز پارامتر پروسجور
            string cacheKey = $"{conn.ConnectionString}:{storedProcedureName}";

            return _cache.GetOrAdd(cacheKey, _ =>
            {
                // اگر در کش نبود، از دیتابیس می‌خونیم
                using var cmd = new SqlCommand(storedProcedureName, conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 60;

                SqlCommandBuilder.DeriveParameters(cmd);

                // فقط ساختار پارامترها، بدون ReturnValue
                var discoveredParams = cmd.Parameters
                    .Cast<SqlParameter>()
                    .Where(p => p.Direction != ParameterDirection.ReturnValue)
                    .Select(p =>
                    {
                        var clone = (SqlParameter)((ICloneable)p).Clone();
                        clone.Value = DBNull.Value; // مهم: مقدار خالی
                        return clone;
                    })
                    .ToArray();

                return discoveredParams;
            })
            // ⚠️ مهم: اینجا دوباره Clone می‌کنیم که نسخه کش دستکاری نشه
            .Select(p => (SqlParameter)((ICloneable)p).Clone())
            .ToArray();
        }

        public static async Task RunCommandAsync(string procName, Dictionary<string, object> inputValues = null)
        {
            using (SqlConnection conn = GetApartemaConnection())
            {
                if (conn.State != ConnectionState.Open)
                    await conn.OpenAsync(); 

                using (SqlCommand cmd = new SqlCommand(procName, conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 60;

                    var derivedParams = GetCachedParameters(conn, procName);
                    cmd.Parameters.AddRange(derivedParams);

                    if (inputValues != null)
                    {
                        foreach (SqlParameter param in cmd.Parameters)
                        {
                            // فقط برای پارامترهای ورودی مقدار بگذار
                            if (param.Direction == ParameterDirection.Input)
                            {
                                if (inputValues != null && inputValues.TryGetValue(param.ParameterName, out object value))
                                    param.Value = value ?? DBNull.Value;
                                else
                                    param.Value = DBNull.Value;     
                            }
                        }
                    }

                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }
        public static async Task<string> GetReportResultAsync(string procName, Dictionary<string, object> inputValues = null, string Res = "")
        {
            var rowData = await GetReportRowAsync(procName, inputValues);
            return rowData[Res].ToString();
        }


        public static async Task<DataTable> GetReportDataAsync(string ComText)
        {
            DataTable repTable = new("RES");

            using (SqlConnection conn = GetApartemaConnection())
            {
                if (conn.State != ConnectionState.Open)
                    await conn.OpenAsync();

                using SqlCommand cmd = new SqlCommand(ComText, conn);

                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    repTable.Load(reader);
                }
            }
            return repTable;
        }

        public static async Task<DataRow> GetReportRowAsync(string procName, Dictionary<string, object> inputValues = null)
        {
            var listData = await DBS.GetReportAsync(procName, inputValues);

            return listData.Rows.Count > 0 ? listData.Rows[0] : null;
        }
        public static async Task<DataTable> GetReportAsync(string procName, Dictionary<string, object> inputValues = null)
        {
            DataTable repTable = new("RES");

            using (SqlConnection conn = GetApartemaConnection())
            {
                if (conn.State != ConnectionState.Open)
                    await conn.OpenAsync();

                using (SqlCommand cmd = new SqlCommand(procName, conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 180;

                    var derivedParams = GetCachedParameters(conn, procName);
                    cmd.Parameters.AddRange(derivedParams);

                    if (inputValues != null)
                    {
                        foreach (SqlParameter param in cmd.Parameters)
                        {
                            // فقط برای پارامترهای ورودی مقدار بگذار
                            if (inputValues.TryGetValue(param.ParameterName, out object value))
                                param.Value = value ?? DBNull.Value;
                            else
                                param.Value = DBNull.Value;
                        }
                    }

                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        repTable.Load(reader);
                    }

                }
            }
            return repTable;
        }

        public static async Task<string> GetReportJsonAsync(string procName, Dictionary<string, object> inputValues = null)
        {//بازگشت جیسون یک گزارش
            var res = await GetReportRowAsync(procName, inputValues);
            string jsondata = JsonConvert.SerializeObject(res, new DataRowJsonConverter());
            return jsondata;
        }
        public static async Task<object> ExecuteScalarAsync(string procName, Dictionary<string, object> inputValues = null)
        {
            using var conn = GetApartemaConnection();
            if (conn.State != ConnectionState.Open)
                await conn.OpenAsync();

            using var cmd = new SqlCommand(procName, conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandTimeout = 60;

            var derivedParams = GetCachedParameters(conn, procName);
            cmd.Parameters.AddRange(derivedParams);

            return await cmd.ExecuteScalarAsync();
        }

        public static async Task LogErrorSaveCmdAsync(string ComText, int userId, int Frm, string Prc, string Err)
        { // ذخیره خطا
            var mainparameters = new Dictionary<string, object>
            {
                { "@PersonId", userId },
                { "@FormId", Frm },
                { "@Prc", Prc },
                { "@Comm", ComText },
                { "@Err", Err }
            };
            await RunCommandAsync("LogError_Save", mainparameters);
        }
        public static async Task<string> LogErrorSaveAsync(string procName, Dictionary<string, object> parameters = null, int userId = 0, int Frm = 0, string Prc = "", string Err = "")
        { // ذخیره خطا
            string ComText = Center.CommandText(procName, parameters);
            await LogErrorSaveCmdAsync(ComText, userId, Frm, Prc, Err);

            return ComText;
        }
        public static async Task TestProcedure(string procName, Dictionary<string, object> parameters = null)
        { // متن دستور SQL برای اجرا
            await LogErrorSaveAsync(procName, parameters, 0, 0, "", "Test Procedure");
        }

        public static async Task<bool> CheckToken(string Token, UserInfo userInfo)
        { // کنترل اینکه آیا کاربر لاگین کرده است؟
            try
            {

                if (string.IsNullOrWhiteSpace(Token) ||
                    !Guid.TryParse(Token, out Guid authToken))
                {
                    userInfo.Id = 0;
                    return false;
                }

                Dictionary<string, object> Param = new Dictionary<string, object>
                {
                    { "@Token", authToken},
                };
                var chektoken = await GetReportRowAsync("dbo.CheckToken", Param);
                
                if (chektoken != null) {
                    if (chektoken.Field<bool>("IsActive"))
                    {
                        userInfo.Id = int.Parse(chektoken["PersonId"].ToString());
                        userInfo.Complex = int.Parse(chektoken["ComplexId"].ToString());
                        userInfo.Name = chektoken["FullName"].ToString();
                        return true;
                    }
                    else
                    {
                        userInfo.Id = 0;
                        return false;
                    }
                }
                else
                {
                    userInfo.Id = 0;
                    return false;
                }
            }
            catch
            {
                userInfo.Id = 0;
                return false;
            };

        }
 
        public static async Task<string> Emruzasync()
        {
            return await GetReportResultAsync("TodayShamsi", null, "Today");
        }

        //public static async Task<string> DateSegmantAsync(string _Date)
        //{ // بازگرداندن اجزای تاریخ
        //    return await GetReportResultAsync("GetDate", null, _Date);
        //}

        //public static async Task<DataRow> DateSegmantAsync()
        //{ // بازگرداندن اجزای تاریخ
        //    return await GetReportRowAsync("GetDate", null);
        //}
        public static async Task<FormData> GetFormData(int UserSi, int FormId)
        {
            // ثبت آخرین فرم ورودی و گرفتن تعداد پیامهای باز کاربر
            var paramLastVisit = new Dictionary<string, object>
            {
                { "@PersonId", UserSi },
                { "@FormId",FormId }
            };
            await DBS.GetReportRowAsync("UserFormAmar_UpdateTime", paramLastVisit);

            FormData formData = new();

            var paramlastForms = new Dictionary<string, object>
            {
                { "@PersonId", UserSi }
            };
            formData.lastForms = await GetReportAsync("UserForms_LastForms", paramlastForms);

            var paramPages = new Dictionary<string, object>
            {
                { "@FormId", FormId }
            };
            formData.Pages = await GetReportAsync("FormPages_List", paramPages);

            return formData;
        }
        public static async Task<string> FindValueAsync(string Table, Int32 SiValue, string FiledResult)
        {
            string Result = "";
            using (SqlConnection conn = GetApartemaConnection())
            {
                if (conn.State != ConnectionState.Open)
                    await conn.OpenAsync();

                using (SqlCommand cmd = new SqlCommand("FindValue", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 60;

                    cmd.Parameters.AddWithValue("@Table", Table);
                    cmd.Parameters.AddWithValue("@Id", SiValue);
                    cmd.Parameters.AddWithValue("@Filed", FiledResult);

                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            Result = reader["Res"].ToString();
                        }
                    }
                }
            }

            return Result;
        }

    }

    public class PasswordService
    {
        private static readonly PasswordHasher<string> _hasher = new();

        public static string HashPassword(string password)
        {
            return _hasher.HashPassword(null, password);
        }

        public static bool VerifyPassword(string hashedPassword, string inputPassword)
        {
            var result = _hasher.VerifyHashedPassword(null, hashedPassword, inputPassword);
            return result == PasswordVerificationResult.Success;
        }
    }

}
