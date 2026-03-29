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
        public required DataTable ListProducts { get; set; }
        public required DataTable ListCategoris { get; set; }
        public required DataRow RowData;


        public async Task<IActionResult> OnGet()
        {
            try
            {
                await DBS.CheckToken(Center.TrimTxt(Request.Cookies["Aparteman.ir"]), userInfo);
                if (userInfo.Id == 0) return RedirectToPage("/Login");

                formData = await DBS.GetFormData(userInfo.Id, FormId);

                return Page();
            }
            catch (Exception ex)
            {
                ViewData["ERRComm"] = await DBS.LogErrorSaveAsync(ProcName, Params, Center.GetUserId(Request.Cookies["User.Aparteman.ir"]), FormId, MethodBase.GetCurrentMethod().Name, ex.Message);
                return RedirectToPage("/Other/Khata", new { returnUrl = URL });
            }
        }

        public async Task<IActionResult> OnPostSendToSabad(string ProdID , string NQ, string Sharh)
        {//افزودن کالا به سبد
            try
            {
                if (!await DBS.CheckToken(Center.TrimTxt(Request.Cookies["Aparteman.ir"]), userInfo))
                    return RedirectToPage("/LoginFast");

                Params = new Dictionary<string, object>
                {
                    { "@Id_Customer", userInfo.Id } ,
                    { "@Id_Product", ProdID },
                    { "@Nq", NQ },
                    { "@Sharh", Sharh },
                };
                ProcName = "Factor_Add";
                return Content(await DBS.GetReportResultAsync(ProcName, Params, "ResTp"));
            }
            catch (Exception ex)
            {
                ViewData["ERRComm"] = await DBS.LogErrorSaveAsync(ProcName, Params, Center.GetUserId(Request.Cookies["User.Aparteman.ir"]), FormId, MethodBase.GetCurrentMethod().Name, ex.Message);
                ViewData["ERRRes"] = ex.Message;
                ViewData["Switch"] = "Error"; VU = "_CommonView";
            }

            return new PartialViewResult
            {
                ViewName = VU,
                ViewData = this.ViewData
            };

        }

        public async Task<IActionResult> OnGetFormProductInfo(string ProdID)
        {//فرم اطلاعات کامل کالا
            try
            {
                if (!await DBS.CheckToken(Center.TrimTxt(Request.Cookies["Aparteman.ir"]), userInfo))
                    return RedirectToPage("/LoginFast");
                
                ViewData["Switch"] = "FormProductInfo";

                Params = new Dictionary<string, object>
                {
                    { "@Si", ProdID },
                };
                ProcName = "Products_Info";
                RowData = await DBS.GetReportRowAsync(ProcName, Params);
            }
            catch (Exception ex)
            {
                ViewData["ERRComm"] = await DBS.LogErrorSaveAsync(ProcName, Params, Center.GetUserId(Request.Cookies["User.Aparteman.ir"]), FormId, MethodBase.GetCurrentMethod().Name, ex.Message);
                ViewData["ERRRes"] = ex.Message;
                ViewData["Switch"] = "Error"; VU = "_CommonView";
            }

            return new PartialViewResult
            {
                ViewName = VU,
                ViewData = this.ViewData
            };

        }
        public async Task<IActionResult> OnGetFormSendToSabad(string ProdID)
        {//افزودن کالا به سبد
            try
            {
                if (!await DBS.CheckToken(Center.TrimTxt(Request.Cookies["Aparteman.ir"]), userInfo))
                    return RedirectToPage("/LoginFast");

                ViewData["Switch"] = "FormSendToSabad";

                ViewData["ProdID"] = ProdID;
            }
            catch (Exception ex)
            {
                ViewData["ERRComm"] = await DBS.LogErrorSaveAsync(ProcName, Params, Center.GetUserId(Request.Cookies["User.Aparteman.ir"]), FormId, MethodBase.GetCurrentMethod().Name, ex.Message);
                ViewData["ERRRes"] = ex.Message;
                ViewData["Switch"] = "Error"; VU = "_CommonView";
            }

            return new PartialViewResult
            {
                ViewName = VU,
                ViewData = this.ViewData
            };

        }

        public async Task<IActionResult> OnGetSearchKala(string Category, string Prod)
        {// لیست جستجو
            try
            {
                ViewData["Switch"] = "ListProducts";

                // کالاهای جستجو شده
                Params = new Dictionary<string, object>
                {
                    { "@Category", Category },
                    { "@Prod", Prod }
                };
                ProcName = "Products_ListSearh";
                ListProducts = await DBS.GetReportAsync(ProcName, Params);
            }
            catch (Exception ex)
            {
                ViewData["ERRComm"] = await DBS.LogErrorSaveAsync(ProcName, Params, Center.GetUserId(Request.Cookies["User.Aparteman.ir"]), FormId, MethodBase.GetCurrentMethod().Name, ex.Message);
                ViewData["ERRRes"] = ex.Message;
                ViewData["Switch"] = "Error"; VU = "_CommonView";
            }

            return new PartialViewResult
            {
                ViewName = VU,
                ViewData = this.ViewData
            };
        }

    }
}