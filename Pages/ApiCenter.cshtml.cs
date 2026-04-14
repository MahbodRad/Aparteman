using Aparteman.Models;
using Aparteman.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using NPOI.OpenXmlFormats.Wordprocessing;

namespace Aparteman.Pages
{
    public class ApiCenterModel : PageModel
    {

        private string ProcName = ""; private Dictionary<string, object> Params;
        private string URL = "/ApiCenter";
        public Int32 FormId = 118;
        private string VU = "ApiCenter_View";
        public UserInfo userInfo = new UserInfo();

        public DataRow RowData;
        public DataTable ListData;
        private Microsoft.AspNetCore.Hosting.IWebHostEnvironment _hostingEnvironment;

        public ApiCenterModel(Microsoft.AspNetCore.Hosting.IWebHostEnvironment hostingEnvironment)
        {// مسیر روت برنامه
            _hostingEnvironment = hostingEnvironment;
        }

        public void OnGet()
        {
        }
        public async Task<IActionResult> OnGetHelpShow(string ID = "0")
        {
            try
            {
                int RJ = 0;
                //******** راهنمای کلی اولیه
                Params = new Dictionary<string, object>
                {
                    { "@FormId", ID},
                };
                ProcName = "Forms_Help";
                RowData = await DBS.GetReportRowAsync(ProcName, Params);

                //********  راهنمای صفحات
                Params = new Dictionary<string, object>
                {
                    { "@FormId", ID},
                };
                ProcName = "Forms_HelpPages";
                ListData = await DBS.GetReportAsync(ProcName, Params);



                ViewData["Switch"] = "Help";
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
        public async Task<IActionResult> OnGetLimitDate(string LimitDateKnd = "0")
        {
            LimitDate _LimitDate;
            try
            {
                Params = new Dictionary<string, object>
                {
                    { "@LimitDate", LimitDateKnd},
                };
                ProcName = "LimitDateFind";
                var DlimitDate = await DBS.GetReportRowAsync(ProcName, Params);
                {
                    _LimitDate = new LimitDate()
                    {
                        FromDate = DlimitDate["FromDate"].ToString(),
                        ToDate = DlimitDate["ToDate"].ToString(),
                    };
                };
            }
            catch (Exception ex)
            {
                ViewData["ERRComm"] = await DBS.LogErrorSaveAsync(ProcName, Params, Center.GetUserId(Request.Cookies["User.Aparteman.ir"]), FormId, MethodBase.GetCurrentMethod().Name, ex.Message);
                ViewData["ERRRes"] = ex.Message;
                ViewData["Switch"] = "Error"; VU = "_CommonView";
                _LimitDate = new LimitDate()
                {
                    FromDate = "خطا",
                    ToDate = "خطا",
                };
            }
            return new JsonResult(_LimitDate);
        }

        public async Task<IActionResult> OnGetListFloors(string BuildingId, string All)
        {//لیست طبقات
            try
            {
                var userId = Center.GetUserId(Request.Cookies["User.Aparteman.ir"]);

                // لیست طبقات
                Params = new Dictionary<string, object>
                {
                    { "@PersonId", userId },
                    { "@BuildingId", BuildingId },
                };
                ProcName = "dbo.Floors_List";
                ListData = await DBS.GetReportAsync(ProcName, Params);

                if (All == "All")
                    ViewData["Switch"] = "SelectFloorAll";
                else
                    ViewData["Switch"] = "SelectFloor";

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

        public async Task<IActionResult> OnGetListUnits(string FloorId, string All)
        {//لیست واحدهای طبقه
            try
            {
                var userId = Center.GetUserId(Request.Cookies["User.Aparteman.ir"]);

                // لیست طبقات
                Params = new Dictionary<string, object>
                {
                    { "@UserId", userId },
                    { "@FloorId", FloorId },
                };
                ProcName = "dbo.Units_List";
                ListData = await DBS.GetReportAsync(ProcName, Params);

                if (All == "All")
                    ViewData["Switch"] = "SelectUnitAll";
                else
                    ViewData["Switch"] = "SelectUnit";

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




        public async Task<IActionResult> OnGetCheckNewMsg(string UserId = "0")
        {
            try
            {
                Params = new Dictionary<string, object>
                {
                    { "@Id_User", UserId},
                };
                ProcName = "MsgDt_CountNew";
                var res = await DBS.GetReportRowAsync(ProcName,Params);
                return Content(res["Cnt"].ToString());
            }
            catch
            {
                return Content("0");
            }
        }

    }
}
