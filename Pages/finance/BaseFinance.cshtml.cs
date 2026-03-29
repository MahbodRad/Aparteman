using Aparteman.Models;
using Aparteman.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NPOI.SS.Formula.Functions;
using System.Data;
using System.Reflection;

namespace Aparteman.Pages.finance
{
    public class BaseFinanceModel : PageModel
    {
        private string ProcName = ""; private Dictionary<string, object> Params;
        private string VU = "BaseFinance_View";
        public Int32 FormId = 5;

        public UserInfo userInfo = new UserInfo();
        public required FormData formData;

        public required DataTable ListItems;
        public required DataTable ListPeriods;


        public required DataTable ListAccount;
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

                return Page();
            }
            catch (Exception ex)
            {
                ViewData["ERRComm"] = await DBS.LogErrorSaveAsync(ProcName, Params, Center.GetUserId(Request.Cookies["User.Aparteman.ir"]), FormId, MethodBase.GetCurrentMethod().Name, ex.Message);
                return RedirectToPage("/Other/Khata", new { returnUrl = Url.PageLink() });
            }
        }


        public async Task<IActionResult> OnGetRepListAccounts(string AccountTitle, string AccountType, string DetailCode, string IsActive, string Sort)
        {//گزارش حساب ها
            try
            {
                var userId = Center.GetUserId(Request.Cookies["User.Aparteman.ir"]);

                // لیست حسابها
                Params = new Dictionary<string, object>
                {
                    { "@UserId", userId } ,
                    { "@AccountTitle", AccountTitle },
                    { "@AccountTypeCode", AccountType },
                    { "@DetailCode", DetailCode },
                    { "@IsActive", IsActive },
                    { "@Sort", Sort },
                };
                ProcName = "dbo.Accounts_List";
                ListItems = await DBS.GetReportAsync(ProcName, Params);

                if (ListItems.Rows.Count == 0)
                {
                    ViewData["Switch"] = "MSG";
                    ViewData["MSG"] = "حسابی با مشخصات فوق پیدا نشد. شرایط جستجو را تغییر بدهید و دوباره گزارش بگیرید";

                    return new PartialViewResult
                    {
                        ViewName = "_CommonView",
                        ViewData = this.ViewData
                    };
                }
                ViewData["Switch"] = "RepListAccounts";

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

        public async Task<IActionResult> OnGetFormSabtAccount(string ID = "0")
        {// فرم ثبت حساب
            try
            {
                ViewData["ID"] = ID;
                var UserId = Center.GetUserId(Request.Cookies["User.Aparteman.ir"]);
                if (ID != "0")
                {
                    Params = new Dictionary<string, object>
                    {
                        { "@UserId" , UserId } ,
                        { "@AccountId" , ID } ,
                    };
                    ProcName = "dbo.Accounts_Info";
                    RowData = await DBS.GetReportRowAsync(ProcName, Params);

                    if (RowData["ComplexId"].ToString() == "")
                    {
                        ViewData["Switch"] = "MSG";
                        ViewData["MSG"] = "امکان ویرایش حساب های پایه را ندارید";

                        return new PartialViewResult
                        {
                            ViewName = "_CommonView",
                            ViewData = this.ViewData
                        };
                    }
                }

                ViewData["Switch"] = "FormSabtAccount";
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
        public async Task<IActionResult> OnPostUpdateAccount(string ID, string AccountCode, string AccountTitle, string AccountTypeCode, string DetailCode, string IsActive)
        {//ثبت یک هزینه برای شارژ
            try
            {
                if (!await DBS.CheckToken(Center.TrimTxt(Request.Cookies["Aparteman.ir"]), userInfo))
                    return RedirectToPage("/LoginFast");

                Params = new Dictionary<string, object>
                {
                    { "@UserId", userInfo.Id } ,
                    { "@AccountId", ID },
                    { "@AccountCode", AccountCode },
                    { "@AccountTitle", AccountTitle },
                    { "@AccountTypeCode", AccountTypeCode },
                    { "@DetailCode", DetailCode },
                    { "@IsActive", IsActive },
                };

                ProcName = "dbo.Account_Save";
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











        public async Task<IActionResult> OnGetRepListItems(string ItemTitle, string MethodCode, string AccountTitle
                    , string PayerCode, string IsActive, string Sort)
        {//گزارش آیتمها
            try
            {
                var userId = Center.GetUserId(Request.Cookies["User.Aparteman.ir"]);

                // لیست آیتمهای شارژ
                Params = new Dictionary<string, object>
                {
                    { "@UserId", userId } ,
                    { "@ItemTitle", ItemTitle },
                    { "@MethodCode", MethodCode },
                    { "@AccountTitle", AccountTitle },
                    { "@PayerCode", PayerCode },
                    { "@Sort", Sort },
                };
                ProcName = "dbo.ChargeItems_List";
                ListItems = await DBS.GetReportAsync(ProcName, Params);

                if (ListItems.Rows.Count == 0)
                {
                    ViewData["Switch"] = "MSG";
                    ViewData["MSG"] = "آیتم شارژی با مشخصات فوق پیدا نشد. شرایط جستجو را تغییر بدهید و دوباره گزارش بگیرید";

                    return new PartialViewResult
                    {
                        ViewName = "_CommonView",
                        ViewData = this.ViewData
                    };
                }
                ViewData["Switch"] = "RepListItems";

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

        public async Task<IActionResult> OnGetFormSabtItem(string ID = "0")
        {// فرم ثبت دیتای آیتم شارژ
            try
            {
                ViewData["ID"] = ID;
                var UserId = Center.GetUserId(Request.Cookies["User.Aparteman.ir"]);
                if (ID != "0")
                {
                    Params = new Dictionary<string, object>
                    {
                        { "@UserId" , UserId } ,
                        { "@ItemId" , ID } ,
                    };
                    ProcName = "dbo.ChargeItems_Info";
                    RowData = await DBS.GetReportRowAsync(ProcName, Params);
                }

                Params = new Dictionary<string, object>
                {
                    { "@UserId" , UserId } ,
                    { "@AccountTypeCode" , 4 } ,
                };
                ProcName = "dbo.Accounts_ListActive";
                ListAccount = await DBS.GetReportAsync(ProcName, Params);

                ViewData["Switch"] = "FormSabtItem";
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

        public async Task<IActionResult> OnPostUpdateChargeItem(string ID, string ItemTitle, string MethodCode, string PayerCode, string AccountId, string IsActive, string Sharh)
        {//ثبت یک هزینه برای شارژ
            try
            {
                if (!await DBS.CheckToken(Center.TrimTxt(Request.Cookies["Aparteman.ir"]), userInfo))
                    return RedirectToPage("/LoginFast");

                Params = new Dictionary<string, object>
                {
                    { "@UserId", userInfo.Id } ,
                    { "@ItemId", ID },
                    { "@ItemTitle", ItemTitle },
                    { "@MethodCode", MethodCode },
                    { "@AccountId", AccountId },
                    { "@PayerCode", PayerCode },
                    { "@Sharh", Sharh },
                    { "@IsActive", IsActive },
                };

                ProcName = "dbo.ChargeItems_Save";
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






        public async Task<IActionResult> OnGetFormSabtPeriod(string ID)
        {// فرم ثبت دوره مالی
            try
            {
                ViewData["ID"] = ID;
                var UserId = Center.GetUserId(Request.Cookies["User.Aparteman.ir"]);
                if (ID != "0")
                {
                    Params = new Dictionary<string, object>
                    {
                        { "@UserId" , UserId } ,
                        { "@PeriodId" , ID } ,
                    };
                    ProcName = "dbo.FinancialPeriods_Info";
                    RowData = await DBS.GetReportRowAsync(ProcName, Params);
                }
                ViewData["Today"] = await DBS.Emruzasync();

                ViewData["Switch"] = "FormSabtPeriod";
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
        public async Task<IActionResult> OnPostUpdatePeriod(string ID, string PeriodTitle, string StartDate, string EndDate)
        {//ثبت یک هزینه برای شارژ
            try
            {
                if (!await DBS.CheckToken(Center.TrimTxt(Request.Cookies["Aparteman.ir"]), userInfo))
                    return RedirectToPage("/LoginFast");

                Params = new Dictionary<string, object>
                {
                    { "@UserId", userInfo.Id } ,
                    { "@PeriodId", ID },
                    { "@PeriodTitle", PeriodTitle },
                    { "@StartDate", StartDate },
                    { "@EndDate", EndDate },
                };

                ProcName = "dbo.FinancialPeriods_Save";
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

        public async Task<IActionResult> OnGetRepListPeriods(string PeriodTitle, string PeriodDate, string Situation)
        {//گزارش دوره های مالی
            try
            {
                var userId = Center.GetUserId(Request.Cookies["User.Aparteman.ir"]);

                // لیست دوره های مالی
                Params = new Dictionary<string, object>
                {
                    { "@UserId", userId } ,
                    { "@PeriodTitle", PeriodTitle },
                    { "@PeriodDate", PeriodDate },
                    { "@Situation", Situation },
                };
                ProcName = "dbo.FinancialPeriods_List";
                ListPeriods = await DBS.GetReportAsync(ProcName, Params);

                await DBS.TestProcedure(ProcName, Params);
                if (ListPeriods.Rows.Count == 0)
                {
                    ViewData["Switch"] = "MSG";
                    ViewData["MSG"] = "دوره مالی با مشخصات فوق پیدا نشد. شرایط جستجو را تغییر بدهید و دوباره گزارش بگیرید";

                    return new PartialViewResult
                    {
                        ViewName = "_CommonView",
                        ViewData = this.ViewData
                    };
                }
                ViewData["Switch"] = "RepListPeriods";

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
