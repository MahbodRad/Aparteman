using Aparteman.Models;
using Aparteman.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NPOI.OpenXmlFormats.Wordprocessing;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Threading.Tasks;

namespace Aparteman.Pages
{
    public class HomeModel : PageModel
    {
        private string ProcName = ""; private Dictionary<string, object> Params;
        private string URL = "Home";
        private string VU = "Home_View";
        public Int32 FormId = 1;

        public UserInfo userInfo = new UserInfo();
        public required FormData formData;
        public required DataTable ListElanat { get; set; }
        public required DataTable ListElanatSath { get; set; }
        public required DataRow RowData;


        public async Task<IActionResult> OnGet()
        {
            try
            {
                await DBS.CheckToken(Center.TrimTxt(Request.Cookies["Aparteman.ir"]), userInfo);
                if (userInfo.Id == 0) return RedirectToPage("/Login");

                formData = await DBS.GetFormData(userInfo.Id, FormId);

                // لیست اعلانات
                Params = new Dictionary<string, object>
                {
                    { "@UserId", userInfo.Id },
                };
                ProcName = "dbo.Elanat_List";
                ListElanat = await DBS.GetReportAsync(ProcName, Params);

                return Page();
            }
            catch (Exception ex)
            {
                ViewData["ERRComm"] = await DBS.LogErrorSaveAsync(ProcName, Params, Center.GetUserId(Request.Cookies["User.Aparteman.ir"]), FormId, MethodBase.GetCurrentMethod().Name, ex.Message);
                return RedirectToPage("/Other/Khata", new { returnUrl = URL });
            }
        }

    }
}