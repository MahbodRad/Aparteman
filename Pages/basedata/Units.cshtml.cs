using Aparteman.Models;
using Aparteman.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NPOI.SS.Formula.Functions;
using NPOI.SS.Formula.PTG;
using System.Data;
using System.Reflection;

namespace Aparteman.Pages.basedata
{
    public class UnitsModel : PageModel
    {
        private string ProcName = ""; private Dictionary<string, object> Params;
        private string VU = "Units_View";
        public Int32 FormId = 3;

        public UserInfo userInfo = new UserInfo();
        public required FormData formData;

        public required DataTable ListComplex;
        public required DataTable ListBuilding;
        public required DataTable ListFloor;
        public required DataTable ListUnit;

        public required DataRow RowData;


        public async Task<IActionResult> OnGet()
        {
            try
            {
                await DBS.CheckToken(Center.TrimTxt(Request.Cookies["Aparteman.ir"]), userInfo);
                if (userInfo.Id == 0) return RedirectToPage("/Login");

                formData = await DBS.GetFormData(userInfo.Id, FormId);

                // لیست ساختمانها
                Params = new Dictionary<string, object>
                {
                    { "@PersonId", userInfo.Id },
                };
                ProcName = "dbo.Buildings_ListPer";
                ListBuilding = await DBS.GetReportAsync(ProcName, Params);

                if (ListBuilding.Rows.Count == 1)
                {
                    // لیست طبقات
                    Params = new Dictionary<string, object>
                    {
                        { "@PersonId", userInfo.Id },
                        { "@BuildingId", ListBuilding.Rows[0]["BuildingId"] },
                    };
                    ProcName = "dbo.Floors_List";
                    ListFloor = await DBS.GetReportAsync(ProcName, Params);
                }

                return Page();
            }
            catch (Exception ex)
            {
                ViewData["ERRComm"] = await DBS.LogErrorSaveAsync(ProcName, Params, Center.GetUserId(Request.Cookies["User.Aparteman.ir"]), FormId, MethodBase.GetCurrentMethod().Name, ex.Message);
                return RedirectToPage("/Other/Khata", new { returnUrl = Url.PageLink() });
            }
        }
        public async Task<IActionResult> OnGetFormSabtUnit(string ID)
        {// فرم ثبت دیتای واحد
            try
            {
                var userId = Center.GetUserId(Request.Cookies["User.Aparteman.ir"]);

                ViewData["ID"] = ID;

                if (ID == "0")
                { 
                    // لیست ساختمانها
                    Params = new Dictionary<string, object>
                    {
                        { "@PersonId", userId },
                    };
                    ProcName = "dbo.Buildings_ListPer";
                    ListBuilding = await DBS.GetReportAsync(ProcName, Params);

                    if (ListBuilding.Rows.Count == 1)
                    {
                        // لیست طبقات
                        Params = new Dictionary<string, object>
                        {
                            { "@PersonId", userId },
                            { "@BuildingId", ListBuilding.Rows[0]["BuildingId"] },
                        };
                        ProcName = "dbo.Floors_List";
                        ListFloor = await DBS.GetReportAsync(ProcName, Params);
                    }

                }
                else
                {
                    Params = new Dictionary<string, object>
                    {
                        { "@UnitId" , ID } ,
                        { "@UserId" , userId } ,
                    };
                    ProcName = "dbo.Units_Info";
                    RowData = await DBS.GetReportRowAsync(ProcName, Params);

                    // لیست طبقات
                    Params = new Dictionary<string, object>
                        {
                            { "@PersonId", userId },
                            { "@BuildingId", RowData["BuildingId"] },
                        };
                    ProcName = "dbo.Floors_List";
                    ListFloor = await DBS.GetReportAsync(ProcName, Params);
                }
                ViewData["Switch"] = "FormSabtUnit";
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

        public async Task<IActionResult> OnPostUpdateUnit(string ID, string FloorId, string UnitNumber, string Area, string SharePercent, string Shomine, string NqBed
                    , string NumberBarg, string NumberWater, string NumBerGaz, string NumberPark, string NumberStor, string AreaStore
                    , string NqSaken, string NqCharge, string IsActiveMalek, string IsActiveSaken, string ZaribAsan, string SitCode, string Sharh)
        {//ثبت مشخصات واحد
            try
            {
                if (!await DBS.CheckToken(Center.TrimTxt(Request.Cookies["Aparteman.ir"]), userInfo))
                    return RedirectToPage("/LoginFast");

                Params = new Dictionary<string, object>
                {
                    { "@CurrentUserId", userInfo.Id } ,
                    { "@UnitId", ID },
                    { "@FloorId", FloorId },
                    { "@UnitNumber", UnitNumber },
                    { "@Area", Area },
                    { "@SharePercent", SharePercent },
                    { "@Shomine", Shomine },
                    { "@NqBed", NqBed },
                    { "@NumberBarg", NumberBarg },
                    { "@NumberWater", NumberWater },
                    { "@NumBerGaz", NumBerGaz },
                    { "@NumberPark", NumberPark },
                    { "@NumberStor", NumberStor },
                    { "@AreaStore", AreaStore },
                    { "@NqSaken", NqSaken },
                    { "@NqCharge", NqCharge },
                    { "@IsActiveMalek", IsActiveMalek },
                    { "@IsActiveSaken", IsActiveSaken },
                    { "@SitCode", SitCode },
                    { "@ZaribAsan", ZaribAsan },
                    { "@Sharh", Sharh },
                };
                ProcName = "dbo.Units_Save";
                var RES = await DBS.GetReportRowAsync(ProcName, Params);
                return new JsonResult(RES);

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

        public async Task<IActionResult> OnGetListUnits(string BuildingId, string FloorId, string UnitNumber, string PersonName
                    , string NumberBarg, string NumberWater, string NumBerGaz, string NumberPark, string NumberStor
                    , string IsActiveMalek, string IsActiveSaken, string SitCode)
        {//گزارش واحدها
            try
            {
                var userId = Center.GetUserId(Request.Cookies["User.Aparteman.ir"]);

                // گزارش واحدها
                Params = new Dictionary<string, object>
                {
                    { "@UserId", userId } ,
                    { "@BuildingId", BuildingId },
                    { "@FloorId", FloorId },
                    { "@UnitNumber", UnitNumber },
                    { "@PersonName", PersonName },

                    { "@IsActiveMalek", IsActiveMalek },
                    { "@IsActiveSaken", IsActiveSaken },
                    { "@SitCode", SitCode },

                    { "@NumberPark", NumberPark },
                    { "@NumberStor", NumberStor },
                    { "@NumberWater", NumberWater },
                    { "@NumberBarg", NumberBarg },
                    { "@NumBerGaz", NumBerGaz },

                };
                ProcName = "dbo.Units_Search";
                ListUnit = await DBS.GetReportAsync(ProcName, Params);
                if(ListUnit.Rows.Count == 0)
                {
                    ViewData["Switch"] = "MSG";
                    ViewData["MSG"] = "واحدی با مشخصات فوق پیدا نشد. شرایط جستجو را تغییر بدهید و دوباره گزارش بگیرید";

                    return new PartialViewResult
                    {
                        ViewName = "_CommonView",
                        ViewData = this.ViewData
                    };
                }
                //await DBS.TestProcedure(ProcName, Params);
                ViewData["Switch"] = "ListUnits";

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


        public async Task<IActionResult> OnPostDeleteUnit(string UnitId)
        {// حذف واحد
            try
            {
                if (!await DBS.CheckToken(Center.TrimTxt(Request.Cookies["Aparteman.ir"]), userInfo))
                    return RedirectToPage("/LoginFast");

                Params = new Dictionary<string, object>
                {
                    { "@UserId", userInfo.Id } ,
                    { "@UnitId", UnitId },
                };
                ProcName = "dbo.Units_Delete";
                var RES = await DBS.GetReportRowAsync(ProcName, Params);

                return new JsonResult(RES);
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
