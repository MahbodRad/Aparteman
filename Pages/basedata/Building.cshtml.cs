using Aparteman.Models;
using Aparteman.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NPOI.SS.Formula.Functions;
using System.Collections.Generic;
using System.Data;
using System.Numerics;
using System.Reflection;

namespace Aparteman.Pages.basedata
{
    public class BuildingModel : PageModel
    {
        private string ProcName = ""; private Dictionary<string, object> Params;
        private string VU = "Building_View";
        public Int32 FormId = 2;

        public UserInfo userInfo = new UserInfo();
        public required FormData formData;

        public required DataTable ListComplex;
        public required DataTable ListBuilding;
        public required DataTable ListFloor;

        public required DataRow RowData;


        public async Task<IActionResult> OnGet()
        {
            try
            {
                await DBS.CheckToken(Center.TrimTxt(Request.Cookies["Aparteman.ir"]), userInfo);
                if (userInfo.Id == 0) return RedirectToPage("/Login");

                formData = await DBS.GetFormData(userInfo.Id, FormId);

                // لیست مجتمع های شخص
                Params = new Dictionary<string, object>
                {
                    { "@PersonId", userInfo.Id },
                };
                ProcName = "dbo.Complexes_ListPer";
                ListComplex = await DBS.GetReportAsync(ProcName,Params);

                // لیست ساختمانها
                Params = new Dictionary<string, object>
                {
                    { "@PersonId", userInfo.Id },
                };
                ProcName = "dbo.Buildings_List";
                ListBuilding = await DBS.GetReportAsync(ProcName, Params);

                // لیست طبقات
                Params = new Dictionary<string, object>
                {
                    { "@PersonId", userInfo.Id },
                };
                ProcName = "dbo.Floors_ListPer";
                ListFloor = await DBS.GetReportAsync(ProcName, Params);

                return Page();
            }
            catch (Exception ex)
            {
                ViewData["ERRComm"] = await DBS.LogErrorSaveAsync(ProcName, Params, Center.GetUserId(Request.Cookies["User.Aparteman.ir"]), FormId, MethodBase.GetCurrentMethod().Name, ex.Message);
                return RedirectToPage("/Other/Khata", new { returnUrl = Url.PageLink() });
            }
        }
        public async Task<IActionResult> OnGetFormSabtComplex(string ID = "0")
        {// فرم ثبت دیتای اشخاص
            try
            {
                ViewData["ID"] = ID;


                if (ID != "0")
                {
                    Params = new Dictionary<string, object>
                    {
                        { "@ComplexId" , ID } ,
                    };
                    ProcName = "dbo.Complexes_Info";
                    RowData = await DBS.GetReportRowAsync(ProcName, Params);
                }
                ViewData["Switch"] = "FormSabtComplex";
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

        public async Task<IActionResult> OnPostUpdateComplex(string ID, string ComplexName, string Adress, string Active)
        {//ثبت مشخصات مجتمع
            try
            {
                if (!await DBS.CheckToken(Center.TrimTxt(Request.Cookies["Aparteman.ir"]), userInfo))
                    return RedirectToPage("/LoginFast");

                Params = new Dictionary<string, object>
                {
                    { "@PersonId", userInfo.Id } ,
                    { "@ComplexId", ID },
                    { "@ComplexName", ComplexName },
                    { "@Adress", Adress },
                    { "@IsActive", Active },
                };

                ProcName = "dbo.Complexes_Save";
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

        public async Task<IActionResult> OnGetRefreshComplex()
        {//لیست مجتمع ها
            try
            {
                var userId = Center.GetUserId(Request.Cookies["User.Aparteman.ir"]);
                // لیست مجتمع های شخص
                Params = new Dictionary<string, object>
                {
                    { "@PersonId", userId },
                };
                ProcName = "dbo.Complexes_ListPer";
                ListComplex = await DBS.GetReportAsync(ProcName, Params);

                ViewData["Switch"] = "Complex";
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



        public async Task<IActionResult> OnGetFormSabtBuilding(string ID, string CompID, string CompName)
        {// فرم ثبت دیتای ساختمان
            try
            {
                ViewData["ID"] = ID;
                ViewData["CompID"] = CompID;
                ViewData["CompName"] = CompName;

                if (ID != "0")
                {
                    Params = new Dictionary<string, object>
                    {
                        { "@BuildingId" , ID } ,
                    };
                    ProcName = "dbo.Buildings_Info";
                    RowData = await DBS.GetReportRowAsync(ProcName, Params);
                }
                ViewData["Switch"] = "FormSabtBuilding";
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

        public async Task<IActionResult> OnPostUpdateBuilding(string ID, string ComplexId, string BuildingName, string BuildingCode, string Active)
        {//ثبت مشخصات ساختمان
            try
            {
                if (!await DBS.CheckToken(Center.TrimTxt(Request.Cookies["Aparteman.ir"]), userInfo))
                    return RedirectToPage("/LoginFast");

                Params = new Dictionary<string, object>
                {
                    { "@UserId", userInfo.Id } ,
                    { "@BuildingId", ID },
                    { "@ComplexId", ComplexId },
                    { "@BuildingCode", BuildingCode },
                    { "@BuildingName", BuildingName },
                    { "@IsActive", Active },
                };
     
                ProcName = "dbo.Buildings_Save";
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

        public async Task<IActionResult> OnGetRefreshBuilding()
        {//لیست ساختمان ها
            try
            {
                var userId = Center.GetUserId(Request.Cookies["User.Aparteman.ir"]);

                // لیست ساختمانها
                Params = new Dictionary<string, object>
                {
                    { "@PersonId", userId },
                };
                ProcName = "dbo.Buildings_List";
                ListBuilding = await DBS.GetReportAsync(ProcName, Params);

                ViewData["Switch"] = "Building";
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
   
        public async Task<IActionResult> OnGetListBuildings(string ComplexId)
        {//لیست ساختمان ها
            try
            {
                var userId = Center.GetUserId(Request.Cookies["User.Aparteman.ir"]);

                // لیست ساختمانها
                Params = new Dictionary<string, object>
                {
                    { "@PersonId", userId },
                    { "@ComplexId", ComplexId },
                };
                ProcName = "dbo.Buildings_ListComplex";
                ListBuilding = await DBS.GetReportAsync(ProcName, Params);
                if(ListBuilding.Rows.Count == 0)
                {
                    ViewData["Switch"] = "MSG";
                    ViewData["MSG"] = "برای این مجتمع هنوز ساختمانی تعریف نکرده اید";

                    return new PartialViewResult
                    {
                        ViewName = "_CommonView",
                        ViewData = this.ViewData
                    };
                }


                if (ListBuilding.Rows[0]["success"].ToString().ToUpper() == "NOTOK")
                {
                    ViewData["Switch"] = "MSG";
                    ViewData["MSG"] = ListBuilding.Rows[0]["message"].ToString();

                    return new PartialViewResult
                    {
                        ViewName = "_CommonView",
                        ViewData = this.ViewData
                    };
                }

                ViewData["Switch"] = "Building";
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




        public async Task<IActionResult> OnGetFormSabtFloor(string ID, string BuildID, string BuildName)
        {// فرم ثبت دیتای طبقات
            try
            {
                ViewData["ID"] = ID;
                ViewData["BuildID"] = BuildID;
                ViewData["BuildName"] = BuildName;

                if (ID != "0")
                {
                    Params = new Dictionary<string, object>
                    {
                        { "@FloorId" , ID } ,
                    };
                    ProcName = "dbo.Floors_Info";
                    RowData = await DBS.GetReportRowAsync(ProcName, Params);
                }
                ViewData["Switch"] = "FormSabtFloor";
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

        public async Task<IActionResult> OnPostUpdateFloor(string ID, string BuildingId, string FloorNumber, string FloorTitle, string Active)
        {//ثبت مشخصات طبقه
            try
            {
                if (!await DBS.CheckToken(Center.TrimTxt(Request.Cookies["Aparteman.ir"]), userInfo))
                    return RedirectToPage("/LoginFast");

                Params = new Dictionary<string, object>
                {
                    { "@UserId", userInfo.Id } ,
                    { "@FloorId", ID },
                    { "@BuildingId", BuildingId },
                    { "@FloorNumber", FloorNumber },
                    { "@FloorTitle", FloorTitle },
                    { "@IsActive", Active },
                };

                ProcName = "dbo.Floors_Save";
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

        public async Task<IActionResult> OnGetRefreshFloor()
        {//لیست ساختمان ها
            try
            {
                var userId = Center.GetUserId(Request.Cookies["User.Aparteman.ir"]);

                // لیست ساختمانها
                Params = new Dictionary<string, object>
                {
                    { "@PersonId", userId },
                };
                ProcName = "dbo.Buildings_List";
                ListBuilding = await DBS.GetReportAsync(ProcName, Params);

                ViewData["Switch"] = "Building";
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

        public async Task<IActionResult> OnGetListFloors(string BuildingId)
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
                ListFloor = await DBS.GetReportAsync(ProcName, Params);
                if (ListFloor.Rows.Count == 0)
                {
                    ViewData["Switch"] = "MSG";
                    ViewData["MSG"] = "برای این ساختمان هنوز طبقه ای تعریف نکرده اید";

                    return new PartialViewResult
                    {
                        ViewName = "_CommonView",
                        ViewData = this.ViewData
                    };
                }


                if (ListFloor.Rows[0]["success"].ToString().ToUpper() == "NOTOK")
                {
                    ViewData["Switch"] = "MSG";
                    ViewData["MSG"] = ListFloor.Rows[0]["message"].ToString();

                    return new PartialViewResult
                    {
                        ViewName = "_CommonView",
                        ViewData = this.ViewData
                    };
                }

                ViewData["Switch"] = "Floor";
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
