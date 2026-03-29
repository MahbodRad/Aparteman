using Aparteman.Models;
using Aparteman.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using NPOI.OpenXmlFormats.Wordprocessing;

namespace Aparteman.Pages.Other
{
    public class ReportErrorsModel : PageModel
    {

        private string ProcName = ""; private Dictionary<string, object> Params;
        private string URL = "/Other/ReportErrors";
        private string VU = "ReportErrors_View";
        public Int32 FormId = 13;

        public UserInfo userInfo = new UserInfo();
        public required FormData formData;

        public DataTable ListData;
        public DataTable ListKarbar;

        private Microsoft.AspNetCore.Hosting.IWebHostEnvironment _hostingEnvironment;
        public ReportErrorsModel(Microsoft.AspNetCore.Hosting.IWebHostEnvironment hostingEnvironment)
        {// مسیر روت برنامه
            _hostingEnvironment = hostingEnvironment;
        }

        public async Task<IActionResult> OnGet()
        {
            try
            {
                await DBS.CheckToken(Center.TrimTxt(Request.Cookies["Aparteman.ir"]), userInfo);
                if (userInfo.Id == 0) return RedirectToPage("/Login");

                formData = await DBS.GetFormData(userInfo.Id, FormId);

                ViewData["Today"] = await DBS.Emruzasync();

                Params = null;
                ProcName = "Karbaran_ListActive";
                ListKarbar = await DBS.GetReportAsync(ProcName, Params);
                return Page();
            }
            catch (Exception ex)
            {
                await DBS.LogErrorSaveAsync(ProcName, Params, Center.GetUserId(Request.Cookies["User.Aparteman.ir"]), FormId, MethodBase.GetCurrentMethod().Name, ex.Message);
                return RedirectToPage("/Other/Khata", new { returnUrl = URL });
            }
        }
        public async Task<IActionResult> OnGetFindKarbaran(string KndKarbar)
        {
            try
            {
                Params = new Dictionary<string, object>
                {
                    { "@Knd",KndKarbar }
                };
                ProcName = "Karbaran_ListActiveKnd";
                ListKarbar = await DBS.GetReportAsync(ProcName, Params);

                ViewData["Switch"] = "SelectKarbar";
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
        public async Task<IActionResult> OnGetListLogError(string FromDate, string ToDate,
                                   string KndKarbar, string Karbar, string FrmName, string FunName, string Command)
        {
            try
            {
                if (! await DBS.CheckToken(Center.TrimTxt(Request.Cookies["Aparteman.ir"]), userInfo))
                    return RedirectToPage("/LoginFast");

                Params = new Dictionary<string, object>
                {
                    { "@FromDate", FromDate },
                    { "@ToDate", ToDate },
                    { "@GrpKarbar", KndKarbar },
                    { "@Karbar", Karbar },
                    { "@FrmOnvan", FrmName },
                    { "@FuncName", FunName },
                    { "@Command", Command }
                };
                ProcName = "LogError_Report";
                ListData = await DBS.GetReportAsync(ProcName, Params);
                if (ListData.Rows.Count == 0)
                {
                    ViewData["Switch"] = "MSG";
                    ViewData["MSG"] = "اطلاعاتی پیدا شد";
                    return new PartialViewResult
                    {
                        ViewName = "_CommonView",
                        ViewData = this.ViewData
                    };
                }

                ViewData["Switch"] = "Report";
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
