using Aparteman.Models;
using Aparteman.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Data;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Aparteman.Pages.errors
{
    public class _1000Model : PageModel
    {

        private string URL = "Home";
        private string VU = "Home_View";
        public Int32 FormId = 1;

        public UserInfo userInfo = new UserInfo();
        public FormData formData;


        public async Task<IActionResult> OnGet(string RJ = "", string TabName="")
        {
            try
            {
                await DBS.CheckToken(Center.TrimTxt(Request.Cookies["Aparteman.ir"]), userInfo);
                if (userInfo.Id == 0) return RedirectToPage("/Login");

                formData = await DBS.GetFormData(userInfo.Id, FormId);

                ViewData["RJ"] = RJ;
                ViewData["TabName"] = TabName;

                return Page();
            }
            catch (Exception ex)
            {
                return RedirectToPage("/errors/Khata", new { returnUrl = URL });
            }
        }

    }
}
