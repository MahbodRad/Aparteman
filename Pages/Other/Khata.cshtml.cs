using Aparteman.Models;
using Aparteman.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System;
using System.Data;
using System.Threading.Tasks;

namespace Aparteman.Pages
{
    public class KhataModel : PageModel
    {
        
        private string URL = "Home";
        public Int32 FormId = 1;

        public UserInfo userInfo = new UserInfo();
        public FormData formData;

      
        private Microsoft.AspNetCore.Hosting.IWebHostEnvironment _hostingEnvironment;

        public async Task<IActionResult> OnGet(string returnUrl = "Home")
        {
            try
            {
                ViewData["_Page"] = returnUrl;

                await DBS.CheckToken(Center.TrimTxt(Request.Cookies["Aparteman.ir"]), userInfo);

                return Page();
            }
            catch
            {
                return RedirectToPage("/LoginFast");
            }
        }
        public IActionResult OnPostReturn(string PAGE)
        {
            return RedirectToPage(PAGE);
        }
    }
}