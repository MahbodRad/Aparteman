using Aparteman.Models;
using Aparteman.Services;
using Azure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
/*
 site Key 6Lecw3csAAAAAFOUrQULLnSRh9zLeQZAljI66_65
 secret Key 6Lecw3csAAAAAPIpTO5zijxkBgAEpGZ6o1JFHdhW
 
 */
namespace Aparteman.Pages
{
    public class RegisterModel : PageModel
    {
        private string ProcName = ""; private Dictionary<string, object> Params;
        private string URL = "/Register";
        private string VU = "";
        public Int32 FormId = 36;
        
        public UserInfo userInfo = new UserInfo();
        public void OnGet()
        {

            //return Page();// RedirectToPage("login");
        }
        public class CaptchaResponse
        {
            public bool success { get; set; }
            public string challenge_ts { get; set; }
            public string hostname { get; set; }
        }
        private async Task<bool> VerifyCaptcha(string token)
        {
            var secret = "6Lecw3csAAAAAPIpTO5zijxkBgAEpGZ6o1JFHdhW";

            using var client = new HttpClient();
            var res = await client.PostAsync(
                $"https://www.google.com/recaptcha/api/siteverify?secret={secret}&response={token}",
                null
            );

            var json = await res.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<CaptchaResponse>(json);

            return data.success;
        }
        //public async Task<IActionResult> OnPostRegister()
        //{
        //    var captchaToken = Request.Form["g-recaptcha-response"];

        //    if (string.IsNullOrEmpty(captchaToken))
        //        return Json(new { success = false, message = "کپچا تأیید نشد" });

        //    if (!await VerifyCaptcha(captchaToken))
        //    {
        //        return Json(new
        //        {
        //            success = false,
        //            message = "تشخیص داده شد که شما ربات هستید"
        //        });
        //    }   // ادامه در مرحله بعد
        //}

        public async Task<IActionResult> OnPostCreateTenantAsync(string tenantName, string contactName, string contactMobile, string contactEmail
                , string complexName, string address, string username, string password)
        {
        
            try
            {
                var passwordHash = PasswordService.HashPassword(password);

                // با یک پروسجور مشتری سیستم ساخته شود
                //  براساس جدول مشتری سیستم مجتمع ساخته 
                // شخص ایجاد شود در مجتمع ایجاد شود
                // به این شخص کاربری داده شود
                // در انتها او را به صفحه لاگین بفرستیم تا وارد سیستم شود
                Params = new Dictionary<string, object>
                {
                    {"@tenantName", tenantName},
                    {"@contactName", contactName},
                    {"@contactMobile", contactMobile},
                    {"@contactEmail", contactEmail},
                    {"@complexName", complexName},
                    {"@address", address},
                    {"@username", username},
                    {"@password", passwordHash},
                };
                ProcName = "dbo.Tenants_AddNew";

                return new JsonResult(await DBS.GetReportRowAsync(ProcName, Params));
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = "notOK", message = "خطا در ایجاد دسترسی. دوباره تلاش کنید و اگر همچنان دچار خطا میشوید با پشتیبانی تماس بگیرید" });
            }
        }
        
        
        public async Task<IActionResult> OnPostLogin(string UserName, string Password, string ReturnPage)
        {
            UserName = UserName?.Trim() ?? "";
            Password = Password?.Trim() ?? "";

            //  یوزر را میفرستم و هش پسورد را میگیرم
            Params = new Dictionary<string, object>
            {
                { "@UserName", UserName }
            };
            ProcName = "dbo.GetForLogin";
            var user = await DBS.GetReportRowAsync(ProcName, Params);

            if (user == null)
                return new JsonResult(new { success = "notOK", message = "نام کاربری یا رمز عبور اشتباه است", gopage="/Login" });

            string clientIp = GetClientIp(HttpContext);

            var passwordHash = user["PasswordHash"].ToString();
            // هش پسورد را با پسوردی که هم اکنون نوشته صحت سنجی میکنم
            if (!PasswordService.VerifyPassword(passwordHash, Password))
            {
                Params = new Dictionary<string, object>
                {
                    { "@PersonId", user["PersonId"] },
                    { "@ClientIp", clientIp },
                    { "@Sharh", "رمز ورود اشتباه وارد شده است" },
                };
                ProcName = "dbo.SaveErrorLogin";
                await DBS.RunCommandAsync(ProcName, Params);

                return new JsonResult(new { success = "notOK", message = "نام کاربری یا رمز عبور اشتباه است", gopage = "/Login" });
            }

            //پسورد را درست وارد کرده است
            if (user["IsActive"].ToString() == "0")
            {
                Params = new Dictionary<string, object>
                {
                    { "@PersonId", user["PersonId"] },
                    { "@ClientIp", clientIp },
                    { "@Sharh", "وضعیت شما فعال نیست با مدیریت تماس بگیرید" },
                };
                ProcName = "dbo.SaveErrorLogin";
                await DBS.RunCommandAsync(ProcName, Params);

                return new JsonResult(new { success = "notOK", message = "وضعیت شما فعال نیست با مدیریت تماس بگیرید", gopage = "/Login" });
            }

            Guid token = Guid.NewGuid();

            //   صدازدن تابع ذخیره توکن
            Params = new Dictionary<string, object>
                {
                    { "@PersonId", user["PersonId"] },
                    { "@AuthToken", token },
                    { "@ClientIp", clientIp },
                };
            ProcName = "dbo.SabtLogin";
            await DBS.RunCommandAsync(ProcName, Params);

            // ✔ رمز درست است → توکن از SQL
            SetLoginCookies(UserName, token.ToString(), user["PersonId"].ToString());

            return new JsonResult(new { success = "OK", message = "ورود موفق به سیستم", gopage = "/Home" });
        }

        private void SetLoginCookies(string UserName, string Token, string ueserSi)
        {
            var option = new CookieOptions
            {
                Path = "/",
                HttpOnly = true,
                Secure = true,
                IsEssential = true,
                Expires = DateTime.Now.AddDays(365)
            };

            Response.Cookies.Append("Aparteman.ir", Token, option);
            //          Response.Cookies.Append("Chap.Aparteman.ir", "Chap_" + si, option);

            Response.Cookies.Append(
                "User.Aparteman.ir",
                new Dictionary<string, string>
                {
                    { "UserName", UserName },
                    { "Si", ueserSi },
//                    { "Kind", kind }
                }.ToLegacyCookieString(),
                option
            );
        }

        private string GetClientIp(HttpContext context)
        {
            if (context.Request.Headers.TryGetValue("CF-Connecting-IP", out var cfIp))
                return cfIp;

            if (context.Request.Headers.TryGetValue("X-Forwarded-For", out var xff))
                return xff.ToString().Split(',')[0].Trim();

            return context.Connection.RemoteIpAddress?.ToString();
        }

    }
}
