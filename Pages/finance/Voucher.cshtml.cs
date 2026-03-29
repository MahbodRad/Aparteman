using Aparteman.Models;
using Aparteman.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Identity.Client;
using NPOI.SS.Formula.Functions;
using System.Data;
using System.Reflection;

namespace Aparteman.Pages.finance
{
    public class VoucherModel : PageModel
    {
        private string ProcName = ""; private Dictionary<string, object> Params;
        private string VU = "Voucher_View";
        public Int32 FormId = 6;

        public UserInfo userInfo = new UserInfo();
        public required FormData formData;

        public required DataTable ListItems;

        public required DataTable ListBuilding;
        public required DataTable ListFloor;
        public required DataTable ListUnit;
        public required DataTable ListAccount;
        public required DataTable ListData;

        public required DataRow RowData;


        public async Task<IActionResult> OnGet()
        {
            try
            {
                await DBS.CheckToken(Center.TrimTxt(Request.Cookies["Aparteman.ir"]), userInfo);
                if (userInfo.Id == 0) return RedirectToPage("/Login");

                formData = await DBS.GetFormData(userInfo.Id, FormId);
                ViewData["Today"] = await DBS.Emruzasync();

                Params = new Dictionary<string, object>
                {
                    { "@UserId" , userInfo.Id } ,
                    { "@AccountTypeCode" , 0 } ,
                };
                ProcName = "dbo.Accounts_ListActive";
                ListAccount = await DBS.GetReportAsync(ProcName, Params);

                Params = new Dictionary<string, object>
                {
                    { "@UserId" , userInfo.Id }
                };
                ProcName = "dbo.ChargeItems_ListActive";
                ListItems = await DBS.GetReportAsync(ProcName, Params);

                return Page();
            }
            catch (Exception ex)
            {
                ViewData["ERRComm"] = await DBS.LogErrorSaveAsync(ProcName, Params, Center.GetUserId(Request.Cookies["User.Aparteman.ir"]), FormId, MethodBase.GetCurrentMethod().Name, ex.Message);
                return RedirectToPage("/Other/Khata", new { returnUrl = Url.PageLink() });
            }
        }

        public async Task<IActionResult> OnGetFormSabtVoucher(string ID = "0")
        {// فرم رویداد مالی
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



        public async Task<IActionResult> OnGetFormSabtCost(string ID = "0")
        {// فرم ثبت هزینه
            try
            {
                ViewData["ID"] = ID;
                var UserId = Center.GetUserId(Request.Cookies["User.Aparteman.ir"]);
                if (ID != "0")
                {
                }

                Params = new Dictionary<string, object>
                {
                    { "@UserId" , UserId }
                };
                ProcName = "dbo.ChargeItems_ListActive";
                ListItems = await DBS.GetReportAsync(ProcName, Params);

                // لیست ساختمانها
                Params = new Dictionary<string, object>
                {
                    { "@PersonId", UserId },
                };
                ProcName = "dbo.Buildings_List";
                ListBuilding = await DBS.GetReportAsync(ProcName, Params);

                ViewData["Switch"] = "FormSabtCost";
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
        public async Task<IActionResult> OnGetSanadSabtCost(string AmountCost, string ItemId, string BuildingId, string FloorId, string UnitIds, string Sharh)
        {// سند تقسیم هزینه
            try
            {
                var UserId = Center.GetUserId(Request.Cookies["User.Aparteman.ir"]);

                AmountCost = Center.CommaClear(AmountCost);
                if (AmountCost == "0")
                {
                    ViewData["Switch"] = "MSG";
                    ViewData["MSG"] = "مبلغ هزینه را بنویسید";

                    return new PartialViewResult
                    {
                        ViewName = "_CommonView",
                        ViewData = this.ViewData
                    };
                }


                // محاسبه تقسیم هزینه
                Params = new Dictionary<string, object>
                {
                    { "@UserId", UserId },
                    { "@AmountCost", AmountCost },
                    { "@ItemId", ItemId },
                    { "@BuildingId", BuildingId },
                    { "@FloorId", FloorId },
                    { "@UnitIds", UnitIds },
                    { "@Sharh", @Sharh },
                };

                ProcName = "dbo.FinanceCalculateCost";
                ListData = await DBS.GetReportAsync(ProcName, Params);
                if (ListData.Rows[0]["success"].ToString().ToUpper() != "OK")
                {
                    ViewData["Switch"] = "MSG";
                    ViewData["MSG"] = ListData.Rows[0]["message"].ToString();

                    return new PartialViewResult
                    {
                        ViewName = "_CommonView",
                        ViewData = this.ViewData
                    };
                }
                ViewData["SecCode"] = ListData.Rows[0]["SecCode"].ToString();

                ViewData["Today"] = await DBS.Emruzasync();
                ViewData["Sharh"] = @Sharh;

                ViewData["Switch"] = "SanadSabtCost";
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
        public async Task<IActionResult> OnPostSodureSanadCost(string DateAction, string Sharh, string SecCode)
        {// صدور سند هزینه
            try
            {
                if (!await DBS.CheckToken(Center.TrimTxt(Request.Cookies["Aparteman.ir"]), userInfo))
                    return RedirectToPage("/LoginFast");

                // صدور سند هزینه
                Params = new Dictionary<string, object>
                {
                    { "@UserId", userInfo.Id },
                    { "@DateAction", DateAction },
                    { "@SecCode", SecCode },
                    { "@Sharh", @Sharh },
                };
                ProcName = "dbo.FinanceSabtCost";
                var SabtCost = await DBS.GetReportRowAsync(ProcName,Params);

                return new JsonResult(SabtCost);

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
        public async Task<IActionResult> OnGetRepListCost(string FromDate, string ToDate, string ItemId, string MethodCode, string PayerCode, string Sharh, string Sort)
        {//لیست هزینه ها
            try
            {
                var userId = Center.GetUserId(Request.Cookies["User.Aparteman.ir"]);

                Params = new Dictionary<string, object>
                {
                    { "@UserId", userId },
                    { "@FromDate", FromDate },
                    { "@ToDate", ToDate },
                    { "@ItemId", ItemId },
                    { "@MethodCode", MethodCode },
                    { "@PayerCode", PayerCode },
                    { "@Sharh", Sharh },
                    { "@Sort", Sort },
                };
                ProcName = "dbo.FinancialEvents_ReportCost";
                ListData = await DBS.GetReportAsync(ProcName, Params);
                if (ListData.Rows.Count == 0)
                {
                    ViewData["Switch"] = "MSG";
                    ViewData["MSG"] = "سند هزینه با توجه به شرایط فوق پیدا نشد. \n شرایط گزارش گیری را تغییر بدهید و دوباره گزارش بگیرید";

                    return new PartialViewResult
                    {
                        ViewName = "_CommonView",
                        ViewData = this.ViewData
                    };

                }

                ViewData["Switch"] = "RepListCost";
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

                ViewData["BuildingId"] = BuildingId;

                if (BuildingId != "0")
                {
                    // لیست طبقات
                    Params = new Dictionary<string, object>
                    {
                        { "@PersonId", userId },
                        { "@BuildingId", BuildingId },
                    };
                    ProcName = "dbo.Floors_List";
                    ListFloor = await DBS.GetReportAsync(ProcName, Params);
                };


                ViewData["Switch"] = "FloorCost";
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
        public async Task<IActionResult> OnGetListUnits(string FloorId)
        {//لیست واحدها
            try
            {
                var userId = Center.GetUserId(Request.Cookies["User.Aparteman.ir"]);
                ViewData["FloorId"] = FloorId;
                if (FloorId != "0")
                {
                    // لیست طبقات
                    Params = new Dictionary<string, object>
                    {
                        { "@UserId", userId },
                        { "@FloorId", FloorId },
                    };
                    ProcName = "dbo.Units_List";
                    ListUnit = await DBS.GetReportAsync(ProcName, Params);
                }

                ViewData["Switch"] = "UnitCost";
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

        public async Task<IActionResult> OnGetListEventItems(string EventId)
        {//لیست هزینه ها
            try
            {
                var userId = Center.GetUserId(Request.Cookies["User.Aparteman.ir"]);

                Params = new Dictionary<string, object>
                {
                    { "@UserId", userId },
                    { "@EventId", EventId },
                };
                ProcName = "dbo.FinancialEventItems_EventId";
                ListData = await DBS.GetReportAsync(ProcName, Params);

                Params = new Dictionary<string, object>
                {
                    { "@UserId", userId },
                    { "@EventId", EventId },
                };
                ProcName = "dbo.FinancialEvents_Info";
                RowData = await DBS.GetReportRowAsync(ProcName, Params);

                ViewData["Switch"] = "EventItems";
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
        public async Task<IActionResult> OnGetListFindTafzili(string Filter, string DetailCode, string REPValue)
        {//لیست اشخاص فیلتر شده
            try
            {
                var userId = Center.GetUserId(Request.Cookies["User.Aparteman.ir"]);

                Params = new Dictionary<string, object>
                {
                    { "@UserId", userId },
                    { "@Filter", Filter },
                    { "@DetailCode", DetailCode },
                };
                ProcName = "dbo.FindTafzili";
                ListData = await DBS.GetReportAsync(ProcName, Params);
                ViewData["Rep"] = REPValue;
                ViewData["DetailCode"] = DetailCode;
                ViewData["Switch"] = "ListTafzili";
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


        public async Task<IActionResult> OnGetFormSabtDaryaft(string ID = "0")
        {// فرم ثبت درآمد/ دریافت
            try
            {
                ViewData["ID"] = ID;
                var UserId = Center.GetUserId(Request.Cookies["User.Aparteman.ir"]);

                if (ID != "0")
                {
                    Params = new Dictionary<string, object>
                    {
                        { "@FinancialEventId" , ID } ,
                        { "@UserId" , UserId } ,
                    };
                    ProcName = "dbo.FinanceSabtDaryaftPardakht_Basedata";
                    RowData = await DBS.GetReportRowAsync(ProcName, Params);
                }

                Params = new Dictionary<string, object>
                {
                    { "@UserId" , UserId } ,
                    { "@AccountTypeCode" , 0 } ,
                };
                ProcName = "dbo.Accounts_ListActive";
                ListAccount = await DBS.GetReportAsync(ProcName, Params);

                ViewData["Today"] = await DBS.Emruzasync();
                ViewData["Switch"] = "FormSabtDaryaft";
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
        public async Task<IActionResult> OnPostUpdateDaryaft(string Amount, string DateAction, string AccountBes, string TafziliBes, string AccountBed, string TafziliBed, string Sharh)
        {// سند ثبت درآمد/ دریافت
            try
            {
                var UserId = Center.GetUserId(Request.Cookies["User.Aparteman.ir"]);

                Amount = Center.CommaClear(Amount);
                if (Amount == "0")
                {
                    ViewData["Switch"] = "MSG";
                    ViewData["MSG"] = "مبلغ دریافتی را بنویسید";

                    return new PartialViewResult
                    {
                        ViewName = "_CommonView",
                        ViewData = this.ViewData
                    };
                }

                // ثبت سند حسابداری دریافت
                Params = new Dictionary<string, object>
                {
                    { "@UserId", UserId },
                    { "@Amount", Amount },
                    { "@DateAction", DateAction },
                    { "@AccountBes", AccountBes },
                    { "@TafziliBes", TafziliBes },
                    { "@AccountBed", AccountBed },
                    { "@TafziliBed", TafziliBed },
                    { "@Sharh", Sharh },
                };

                ProcName = "dbo.FinanceSabtDaryaft";
                var SabtDaryaft = await DBS.GetReportRowAsync(ProcName, Params);

                return new JsonResult(SabtDaryaft);
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
        public async Task<IActionResult> OnGetRepListDaryaft(string FromDate, string ToDate, string AccountId, string TafziliId
                , string Sharh, string PayerCode, string Sort)
        {//لیست دریافت ها
            try
            {
                var userId = Center.GetUserId(Request.Cookies["User.Aparteman.ir"]);

                Params = new Dictionary<string, object>
                {
                    { "@UserId", userId },
                    { "@FromDate", FromDate },
                    { "@ToDate", ToDate },
                    { "@AccountId", AccountId },
                    { "@TafziliId", TafziliId },
                    { "@Sharh", Sharh },
                    { "@Sort", Sort },
                };
                ProcName = "dbo.FinancialEvents_ReportDaryaft";
                await DBS.TestProcedure(ProcName, Params);
                ListData = await DBS.GetReportAsync(ProcName, Params);
                if (ListData.Rows.Count == 0)
                {
                    ViewData["Switch"] = "MSG";
                    ViewData["MSG"] = "سند دریافت با توجه به شرایط فوق پیدا نشد. \n شرایط گزارش گیری را تغییر بدهید و دوباره گزارش بگیرید";

                    return new PartialViewResult
                    {
                        ViewName = "_CommonView",
                        ViewData = this.ViewData
                    };

                }

                ViewData["Switch"] = "RepListDaryaft";
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


        public async Task<IActionResult> OnGetFormSabtPardakht(string ID = "0")
        {// فرم ثبت پرداخت
            try
            {
                ViewData["ID"] = ID;
                var UserId = Center.GetUserId(Request.Cookies["User.Aparteman.ir"]);

                if (ID != "0")
                {
                    Params = new Dictionary<string, object>
                    {
                        { "@FinancialEventId" , ID } ,
                        { "@UserId" , UserId } ,
                    };
                    ProcName = "dbo.FinanceSabtDaryaftPardakht_Basedata";
                    RowData = await DBS.GetReportRowAsync(ProcName, Params);
                }

                Params = new Dictionary<string, object>
                {
                    { "@UserId" , UserId } ,
                    { "@AccountTypeCode" , 0 } ,
                };
                ProcName = "dbo.Accounts_ListActive";
                ListAccount = await DBS.GetReportAsync(ProcName, Params);

                ViewData["Today"] = await DBS.Emruzasync();
                ViewData["Switch"] = "FormSabtPardakht";
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
        public async Task<IActionResult> OnPostUpdatePardakht(string Amount, string DateAction, string AccountBes, string TafziliBes, string AccountBed, string TafziliBed, string Sharh)
        {// سند ثبت پرداخت
            try
            {
                var UserId = Center.GetUserId(Request.Cookies["User.Aparteman.ir"]);

                Amount = Center.CommaClear(Amount);
                if (Amount == "0")
                {
                    ViewData["Switch"] = "MSG";
                    ViewData["MSG"] = "مبلغ دریافتی را بنویسید";

                    return new PartialViewResult
                    {
                        ViewName = "_CommonView",
                        ViewData = this.ViewData
                    };
                }

                // ثبت سند حسابداری پرداخت
                Params = new Dictionary<string, object>
                {
                    { "@UserId", UserId },
                    { "@Amount", Amount },
                    { "@DateAction", DateAction },
                    { "@AccountBes", AccountBes },
                    { "@TafziliBes", TafziliBes },
                    { "@AccountBed", AccountBed },
                    { "@TafziliBed", TafziliBed },
                    { "@Sharh", Sharh },
                };

                ProcName = "dbo.FinanceSabtPardakht";
                var SabtPardakht = await DBS.GetReportRowAsync(ProcName, Params);

                return new JsonResult(SabtPardakht);
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
        public async Task<IActionResult> OnGetRepListPardakht(string FromDate, string ToDate, string AccountId, string TafziliId
                , string Sharh, string PayerCode, string Sort)
        {//لیست پرداخت ها
            try
            {
                var userId = Center.GetUserId(Request.Cookies["User.Aparteman.ir"]);

                Params = new Dictionary<string, object>
                {
                    { "@UserId", userId },
                    { "@FromDate", FromDate },
                    { "@ToDate", ToDate },
                    { "@AccountId", AccountId },
                    { "@TafziliId", TafziliId },
                    { "@Sharh", Sharh },
                    { "@Sort", Sort },
                };
                ProcName = "dbo.FinancialEvents_ReportPardakht";
                await DBS.TestProcedure(ProcName, Params);
                await DBS.TestProcedure(ProcName, Params);
                ListData = await DBS.GetReportAsync(ProcName, Params);
                if (ListData.Rows.Count == 0)
                {
                    ViewData["Switch"] = "MSG";
                    ViewData["MSG"] = "سند پرداخت با توجه به شرایط فوق پیدا نشد. \n شرایط گزارش گیری را تغییر بدهید و دوباره گزارش بگیرید";

                    return new PartialViewResult
                    {
                        ViewName = "_CommonView",
                        ViewData = this.ViewData
                    };

                }

                ViewData["Switch"] = "RepListPardakht";
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
