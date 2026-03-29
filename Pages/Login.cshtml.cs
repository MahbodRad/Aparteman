using Aparteman.Models;
using Aparteman.Services;
using Azure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using NPOI.SS.Formula.Functions;
using System.Data;

namespace Aparteman.Pages
{
    public class LoginModel : PageModel
    {
        private string ProcName = ""; private Dictionary<string, object> Params;
        private string URL = "/Login";
        private string VU = "";
        public Int32 FormId = 36;
        
        public UserInfo userInfo = new UserInfo();
        public void OnGet(string returnUrl = null)
        {
            // از بین بردن توکن قبلی
            try
            {
                Response.Cookies.Delete("Aparteman.ir");
                // Response.Cookies.Delete("Chap.Aparteman.ir");
            }
            catch { };

            ViewData["ReturnPage"] = returnUrl ?? "/Home";
            ViewData["userName"] = "";

            ViewData["ResTp"] = "";

            if (Request.Cookies["User.Aparteman.ir"] != null)
            {
                var legacyCookie = Request.Cookies["User.Aparteman.ir"].FromLegacyCookieString();
                ViewData["UserName"] = legacyCookie["UserName"];
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
            {
                ViewData["ResTp"] = "نام کاربری یا رمز عبور اشتباه است";
                return Page();
            }
            
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

                ViewData["ResTp"] = "نام کاربری یا رمز عبور اشتباه است";
                return Page();
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

                ViewData["ResTp"] = "وضعیت شما فعال نیست با مدیریت تماس بگیرید";
                return Page();
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

            return RedirectToPage(ReturnPage);
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

            Response.Cookies.Append("Aparteman.ir", Token , option);
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
