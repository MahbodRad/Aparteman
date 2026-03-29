using Aparteman.Models;
using Aparteman.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Aparteman.Pages
{
    public class LoginFastModel : PageModel
    {

        private string ProcName = ""; private Dictionary<string, object> Params;
        private string URL = "/LoginFast";
        private string VU = "";
        public Int32 FormId = 120;

        public async Task<IActionResult> OnPostLogin(string Password = "")
        {
            try
            {
                var legacyCookie = Request.Cookies["User.Aparteman.ir"].FromLegacyCookieString();
                string UserName = legacyCookie["UserName"];

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
                    return Content("نام کاربری یا رمز عبور اشتباه است");
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

                    return Content("رمز عبور اشتباه است.");
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

                    return Content("وضعیت شما فعال نیست با مدیریت تماس بگیرید");
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

                return Content("OK");

            }
            catch (Exception ex)
            {
                ViewData["ERRComm"] = await DBS.LogErrorSaveAsync(ProcName, Params, Center.GetUserId(Request.Cookies["User.Aparteman.ir"]), FormId, MethodBase.GetCurrentMethod().Name, ex.Message);
                return RedirectToPage("/Other/Khata", new { returnUrl = URL });
            }
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
