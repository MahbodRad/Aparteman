using Aparteman.Models;
using Aparteman.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;
using System.Reflection;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.SS.Util;
using System;

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

        private IWebHostEnvironment _hostingEnvironment;
        public VoucherModel(IWebHostEnvironment hostingEnvironment)
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

        public async Task<IActionResult> OnGetFormSabtVoucher(string ID)
        {// فرم رویداد مالی
            try
            {
                ViewData["ID"] = ID;

                if (ID != "0")
                {
                    var UserId = Center.GetUserId(Request.Cookies["User.Aparteman.ir"]);
                    Params = new Dictionary<string, object>
                    {
                        { "@UserId" , UserId } ,
                        { "@EventId" , ID } ,
                    };
                    ProcName = "dbo.FinancialEvents_Info";
                    RowData = await DBS.GetReportRowAsync(ProcName, Params);

                    Params = new Dictionary<string, object>
                    {
                        { "@UserId", UserId },
                        { "@EventId", ID },
                    };
                    ProcName = "dbo.FinancialEventItems_EventId";
                    ListData = await DBS.GetReportAsync(ProcName, Params);

                }
                else
                    ViewData["Today"] = await DBS.Emruzasync();
    
                ViewData["Switch"] = "FormSabtVoucher";
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
        public async Task<IActionResult> OnPostUpdateFinancialEvents(IFormFile FileUpload, string ID, string DateAction, string Sharh)
        {// سربرگ سند حسابداری
            try
            {
                if (!await DBS.CheckToken(Center.TrimTxt(Request.Cookies["Aparteman.ir"]), userInfo))
                    return RedirectToPage("/LoginFast");

                // ثبت سند حسابداری دریافت
                Params = new Dictionary<string, object>
                {
                    { "@FinancialEventId", ID },
                    { "@DateAction", DateAction },
                    { "@Sharh", Sharh },
                    { "@UserId", userInfo.Id },
                };
                ProcName = "dbo.FinancialEvents_Save";

                var SabtFinancial = await DBS.GetReportRowAsync(ProcName, Params);
                return new JsonResult(SabtFinancial);
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
        public async Task<IActionResult> OnGetRepListVouchers(string FromDate, string ToDate, string EventType, string AccountBes, string TafziliBes
                    , string AccountBed, string TafziliBed, string Sharh, string Filter, string Sort)
        {//گزارش اسناد حسابداری
            try
            {
                var userId = Center.GetUserId(Request.Cookies["User.Aparteman.ir"]);

                Params = new Dictionary<string, object>
                {
                    { "@UserId", userId },
                    { "@FromDate", FromDate },
                    { "@ToDate", ToDate },
                    { "@EventType", EventType },
                    { "@AccountBes", AccountBes },
                    { "@TafziliBes", TafziliBes },
                    { "@AccountBed", AccountBed },
                    { "@TafziliBed", TafziliBed },
                    { "@Filter", Filter },
                    { "@Sharh", Sharh },
                    { "@Sort", Sort },
                };
                ProcName = "dbo.FinancialEvents_ReportVouchers";
                ListData = await DBS.GetReportAsync(ProcName, Params);
                ViewData["ComText"] = Center.CommandText(ProcName, Params);
                if (ListData.Rows.Count == 0)
                {
                    ViewData["Switch"] = "MSG";
                    ViewData["MSG"] = "سند حسابداری با توجه به شرایط فوق پیدا نشد. \n شرایط گزارش گیری را تغییر بدهید و دوباره گزارش بگیرید";

                    return new PartialViewResult
                    {
                        ViewName = "_CommonView",
                        ViewData = this.ViewData
                    };

                }

                ViewData["Switch"] = "RepListVouchers";
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
        public async Task<IActionResult> OnGetChap_RepListVouchers(string ComText, string Chap, string OnvanChap)
        {
            try
            {
                var ReportData = await DBS.GetReportDataAsync(ComText);

                OnvanChap = (Center.TrimTxt(OnvanChap) == "") ? "گزارش اسناد حسابداری" : OnvanChap;
                string webRootPath = _hostingEnvironment.WebRootPath;
                string FileName = System.IO.Path.Combine(webRootPath, "Uploads", Request.Cookies["Chap.Aparteman.ir"]);

                if (Chap == "CHAP" || Chap == "PDF")
                {
                    FileName += ".pdf";
                    Center.DeleteFileIfExists(FileName);

                    // code in your main method
                    Document.Create(container =>
                    {
                        container.Page(page =>
                        {
                            page.Size(PageSizes.A4);
                            page.Margin(1, Unit.Centimetre);
                            page.PageColor(Colors.White);
                            page.DefaultTextStyle(x => x.FontSize(10));
                            page.ContentFromRightToLeft();

                            static IContainer DefaultCellStyle(IContainer container, string backgroundColor = "")
                            {
                                return container
                                  .Border(1)
                                  .BorderColor(Colors.Grey.Lighten1)
                                  .Background(!string.IsNullOrEmpty(backgroundColor) ? backgroundColor : Colors.White)
                                  .PaddingVertical(7)
                                  .PaddingHorizontal(3);
                            }
                            var RJ = 0;

                            page.Header()
                                .Text(OnvanChap)
                                .SemiBold().FontSize(14).FontColor(Colors.Blue.Medium);


                            page.Content()
                                .PaddingVertical(1, Unit.Centimetre)
                                .Border(1)
                                .Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn(7);
                                        columns.RelativeColumn(8);
                                        columns.RelativeColumn(11);
                                        columns.RelativeColumn(7);
                                        columns.RelativeColumn(12);
                                    });
                                    table.Header(header =>
                                    {
                                        header.Cell().Element(CellStyle).AlignCenter().Text("#").FontSize(11).SemiBold();
                                        header.Cell().Element(CellStyle).Text("تاریخ").FontSize(11).SemiBold();
                                        header.Cell().Element(CellStyle).Text("مبلغ").FontSize(11).SemiBold();
                                        header.Cell().Element(CellStyle).Text("نوع هزینه").FontSize(11).SemiBold();
                                        header.Cell().Element(CellStyle).Text("روش تقسیم").FontSize(11).SemiBold();
                                        header.Cell().Element(CellStyle).Text("شرح").FontSize(11).SemiBold();


                                        // you can extend existing styles by creating additional methods
                                        IContainer CellStyle(IContainer container) => DefaultCellStyle(container, Colors.Grey.Lighten3);
                                    });

                                    foreach (DataRow item in ReportData.Rows)
                                    {
                                        RJ++;
                                        table.Cell().Element(CellStyle).Text(RJ.ToString());
                                        table.Cell().Element(CellStyle).Text(item["EventDate"].ToString());
                                        table.Cell().Element(CellStyle).Text(item["Amount"].ToComma());
                                        table.Cell().Element(CellStyle).Text(item["ItemTitle"].ToString());
                                        table.Cell().Element(CellStyle).Text(item["MethodTitle"].ToString());
                                        table.Cell().Element(CellStyle).Text(item["Sharh"].ToString());
                                    }

                                    IContainer CellStyle(IContainer container) => DefaultCellStyle(container).ShowOnce();
                                });

                            page.Footer()
                                .AlignCenter()
                                .Text(x =>
                                {
                                    x.Span("صفحه");
                                    x.CurrentPageNumber();
                                });
                        });
                    })
                    .GeneratePdf(FileName);

                }
                if (Chap == "XLS")
                {
                    FileName += ".xlsx";
                    Center.DeleteFileIfExists(FileName);

                    string sheetName = "اسناد حسابداری";
                    // فایلی از نوع xlsx
                    IWorkbook workbook = new XSSFWorkbook();
                    //ایجاد شیت در فایل اکسل
                    ISheet sheet = workbook.CreateSheet(sheetName);
                    sheet.IsRightToLeft = true;
                    // ایجاد عنصر سطر
                    IRow row = sheet.CreateRow(1);

                    IFont font = workbook.CreateFont();
                    font.IsBold = true;
                    font.FontHeightInPoints = 14;
                    ICellStyle boldStyle = workbook.CreateCellStyle();
                    boldStyle.SetFont(font);

                    int _Row = 1;
                    sheet.AddMergedRegion(new CellRangeAddress(_Row, _Row + 1, 1, 10));
                    row = sheet.CreateRow(_Row);
                    Center.CellAddData(row, 1, OnvanChap, CellType.String); ;

                    _Row += 3;
                    row = sheet.CreateRow(_Row);
                    Center.CellAddData(row, 1, "تاریخ", CellType.String);
                    Center.CellAddData(row, 2, "شماره سند", CellType.String);
                    Center.CellAddData(row, 3, "مبلغ", CellType.String);
                    Center.CellAddData(row, 4, "نوع هزینه", CellType.String);
                    Center.CellAddData(row, 5, "روش تقسیم", CellType.String);
                    Center.CellAddData(row, 6, "شرح", CellType.String);
                    Center.CellAddData(row, 7, "ویرایشگر", CellType.String);
                    Center.CellAddData(row, 8, "زمان ویرایش", CellType.String);

                    foreach (DataRow item in ReportData.Rows)
                    {
                        _Row += 1;
                        row = sheet.CreateRow(_Row);
                        // پرکردن سلولها
                        Center.CellAddData(row, 1, item["EventDate"].ToString(), CellType.Numeric);
                        Center.CellAddData(row, 2, item["FinancialNum"].ToString(), CellType.String);
                        Center.CellAddData(row, 3, item["Amount"].ToString(), CellType.String);
                        Center.CellAddData(row, 4, item["ItemTitle"].ToString(), CellType.String);
                        Center.CellAddData(row, 5, item["MethodTitle"].ToString(), CellType.String);
                        Center.CellAddData(row, 6, item["Sharh"].ToString(), CellType.String);
                        Center.CellAddData(row, 7, item["EditorName"].ToString(), CellType.String);
                        Center.CellAddData(row, 8, item["TimeEdit"].ToString(), CellType.String);
                    }

                    // سایزبندی 20 ستون در شیت
                    for (int i = 0; i <= 20; i++) sheet.AutoSizeColumn(i, true);

                    using (FileStream stream = new FileStream(FileName, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        workbook.Write(stream);
                    }

                }

            }
            catch (Exception ex)
            {
                if (Chap == "CHAP" || Chap == "PDF")
                    Center.ChapEmpty(_hostingEnvironment.WebRootPath, Request.Cookies["Chap.Aparteman.ir"], ".pdf");
                if (Chap == "XLS")
                    Center.ChapEmpty(_hostingEnvironment.WebRootPath, Request.Cookies["Chap.Aparteman.ir"], ".xlsx");

                await DBS.LogErrorSaveAsync(ProcName, Params, Center.GetUserId(Request.Cookies["User.Aparteman.ir"]), FormId, MethodBase.GetCurrentMethod().Name, ex.Message);
            }
            return Content("OK");
        }



        public async Task<IActionResult> OnGetFormSabtVoucherItem(string EventID, string ItemID, string BedBes)
        {// فرم ثبت آیتم رویداد مالی
            try
            {
                ViewData["EventID"] = EventID;
                ViewData["ItemID"] = ItemID;
                ViewData["Bes"] = "0";
                ViewData["Bed"] = "0";
                if (BedBes.Contains("-")) // بستانکار است مقابل آن بدهکار مقدار بگیرد
                    ViewData["Bed"] = BedBes.Replace("-","");
                else // بدهکار است، مقابل آن بستانکار مقدار بگیرد
                    ViewData["Bes"] = BedBes;

                var UserId = Center.GetUserId(Request.Cookies["User.Aparteman.ir"]);
                Params = new Dictionary<string, object>
                    {
                        { "@UserId" , userInfo.Id } ,
                        { "@AccountTypeCode" , 0 } ,
                    };
                ProcName = "dbo.Accounts_ListActive";
                ListAccount = await DBS.GetReportAsync(ProcName, Params);

                if (ItemID != "0")
                {
                }

                ViewData["Switch"] = "FormSabtVoucherItem";
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
        public async Task<IActionResult> OnPostUpdateVoucherItem(string ItemID, string EventID, string AccountId, string TafziliId, string AmountBes, string AmountBed, string Sharh)
        {// ثبت یک ردیف در سند حسابداری
            try
            {
                if (!await DBS.CheckToken(Center.TrimTxt(Request.Cookies["Aparteman.ir"]), userInfo))
                    return RedirectToPage("/LoginFast");

                AmountBes = Center.CommaClear(AmountBes);
                AmountBed = Center.CommaClear(AmountBed);
                if (AmountBes == "0" & AmountBed == "0")
                {
                    ViewData["Switch"] = "MSG";
                    ViewData["MSG"] = "مبلغ بدهکار یا بستانکار را بنویسید";

                    return new PartialViewResult
                    {
                        ViewName = "_CommonView",
                        ViewData = this.ViewData
                    };
                }
                if (AmountBes != "0" & AmountBed != "0")
                {
                    ViewData["Switch"] = "MSG";
                    ViewData["MSG"] = "فقط بدهکار یا بستانکار میتواند مبلغ داشته باشد";

                    return new PartialViewResult
                    {
                        ViewName = "_CommonView",
                        ViewData = this.ViewData
                    };
                }

                if (AccountId == "0")
                {
                    ViewData["Switch"] = "MSG";
                    ViewData["MSG"] = "حساب را انتخاب کنید";

                    return new PartialViewResult
                    {
                        ViewName = "_CommonView",
                        ViewData = this.ViewData
                    };
                }

                // ثبت ردیف سند حسابداری
                Params = new Dictionary<string, object>
                {
                    { "@FinancialEventId", EventID },
                    { "@EventItemId", ItemID },
                    { "@UserId", userInfo.Id },
                    { "@AccountId", AccountId },
                    { "@TafziliId", TafziliId },
                    { "@Bed", AmountBed },
                    { "@Bes", AmountBes },
                    { "@Babat", Sharh },
                };
                ProcName = "dbo.FinancialEventItems_Save";

                var SabtEventItems = await DBS.GetReportRowAsync(ProcName, Params);

                return new JsonResult(SabtEventItems);
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
                ViewData["ComText"] = Center.CommandText(ProcName, Params);
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
        public async Task<IActionResult> OnGetChap_RepListCost(string ComText, string Chap, string OnvanChap)
        {
            try
            {
                var ReportData = await DBS.GetReportDataAsync(ComText);

                OnvanChap = (Center.TrimTxt(OnvanChap) == "") ? "گزارش هزینه ها" : OnvanChap;
                string webRootPath = _hostingEnvironment.WebRootPath;
                string FileName = System.IO.Path.Combine(webRootPath, "Uploads", Request.Cookies["Chap.Aparteman.ir"]);

                if (Chap == "CHAP" || Chap == "PDF")
                {
                    FileName += ".pdf";
                    Center.DeleteFileIfExists(FileName);

                    // code in your main method
                    Document.Create(container =>
                    {
                        container.Page(page =>
                        {
                            page.Size(PageSizes.A4);
                            page.Margin(1, Unit.Centimetre);
                            page.PageColor(Colors.White);
                            page.DefaultTextStyle(x => x.FontSize(10));
                            page.ContentFromRightToLeft();

                            static IContainer DefaultCellStyle(IContainer container, string backgroundColor = "")
                            {
                                return container
                                  .Border(1)
                                  .BorderColor(Colors.Grey.Lighten1)
                                  .Background(!string.IsNullOrEmpty(backgroundColor) ? backgroundColor : Colors.White)
                                  .PaddingVertical(7)
                                  .PaddingHorizontal(3);
                            }
                            var RJ = 0;

                            page.Header()
                                .Text(OnvanChap)
                                .SemiBold().FontSize(14).FontColor(Colors.Blue.Medium);


                            page.Content()
                                .PaddingVertical(1, Unit.Centimetre)
                                .Border(1)
                                .Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn(7);
                                        columns.RelativeColumn(8);
                                        columns.RelativeColumn(11);
                                        columns.RelativeColumn(7);
                                        columns.RelativeColumn(12);
                                    });
                                    table.Header(header =>
                                    {
                                        header.Cell().Element(CellStyle).AlignCenter().Text("#").FontSize(11).SemiBold();
                                        header.Cell().Element(CellStyle).Text("تاریخ").FontSize(11).SemiBold();
                                        header.Cell().Element(CellStyle).Text("مبلغ").FontSize(11).SemiBold();
                                        header.Cell().Element(CellStyle).Text("نوع هزینه").FontSize(11).SemiBold();
                                        header.Cell().Element(CellStyle).Text("روش تقسیم").FontSize(11).SemiBold();
                                        header.Cell().Element(CellStyle).Text("شرح").FontSize(11).SemiBold();


                                        // you can extend existing styles by creating additional methods
                                        IContainer CellStyle(IContainer container) => DefaultCellStyle(container, Colors.Grey.Lighten3);
                                    });

                                    foreach (DataRow item in ReportData.Rows)
                                    {
                                        RJ++;
                                        table.Cell().Element(CellStyle).Text(RJ.ToString());
                                        table.Cell().Element(CellStyle).Text(item["EventDate"].ToString());
                                        table.Cell().Element(CellStyle).Text(item["Amount"].ToComma());
                                        table.Cell().Element(CellStyle).Text(item["ItemTitle"].ToString());
                                        table.Cell().Element(CellStyle).Text(item["MethodTitle"].ToString());
                                        table.Cell().Element(CellStyle).Text(item["Sharh"].ToString());
                                    }

                                    IContainer CellStyle(IContainer container) => DefaultCellStyle(container).ShowOnce();
                                });

                            page.Footer()
                                .AlignCenter()
                                .Text(x =>
                                {
                                    x.Span("صفحه");
                                    x.CurrentPageNumber();
                                });
                        });
                    })
                    .GeneratePdf(FileName);

                }
                if (Chap == "XLS")
                {
                    FileName += ".xlsx";
                    Center.DeleteFileIfExists(FileName);

                    string sheetName = "لیست هزینه ها";
                    // فایلی از نوع xlsx
                    IWorkbook workbook = new XSSFWorkbook();
                    //ایجاد شیت در فایل اکسل
                    ISheet sheet = workbook.CreateSheet(sheetName);
                    sheet.IsRightToLeft = true;
                    // ایجاد عنصر سطر
                    IRow row = sheet.CreateRow(1);

                    IFont font = workbook.CreateFont();
                    font.IsBold = true;
                    font.FontHeightInPoints = 14;
                    ICellStyle boldStyle = workbook.CreateCellStyle();
                    boldStyle.SetFont(font);

                    int _Row = 1;
                    sheet.AddMergedRegion(new CellRangeAddress(_Row, _Row + 1, 1, 10));
                    row = sheet.CreateRow(_Row);
                    Center.CellAddData(row, 1, OnvanChap, CellType.String); ;

                    _Row += 3;
                    row = sheet.CreateRow(_Row);
                    Center.CellAddData(row, 1, "تاریخ", CellType.String);
                    Center.CellAddData(row, 2, "شماره سند", CellType.String);
                    Center.CellAddData(row, 3, "مبلغ", CellType.String);
                    Center.CellAddData(row, 4, "نوع هزینه", CellType.String);
                    Center.CellAddData(row, 5, "روش تقسیم", CellType.String);
                    Center.CellAddData(row, 6, "شرح", CellType.String);
                    Center.CellAddData(row, 7, "ویرایشگر", CellType.String);
                    Center.CellAddData(row, 8, "زمان ویرایش", CellType.String);

                    foreach (DataRow item in ReportData.Rows)
                    {
                        _Row += 1;
                        row = sheet.CreateRow(_Row);
                        // پرکردن سلولها
                        Center.CellAddData(row, 1, item["EventDate"].ToString(), CellType.Numeric);
                        Center.CellAddData(row, 2, item["FinancialNum"].ToString(), CellType.String);
                        Center.CellAddData(row, 3, item["Amount"].ToString(), CellType.String);
                        Center.CellAddData(row, 4, item["ItemTitle"].ToString(), CellType.String);
                        Center.CellAddData(row, 5, item["MethodTitle"].ToString(), CellType.String);
                        Center.CellAddData(row, 6, item["Sharh"].ToString(), CellType.String);
                        Center.CellAddData(row, 7, item["EditorName"].ToString(), CellType.String);
                        Center.CellAddData(row, 8, item["TimeEdit"].ToString(), CellType.String);
                    }

                    // سایزبندی 20 ستون در شیت
                    for (int i = 0; i <= 20; i++) sheet.AutoSizeColumn(i, true);

                    using (FileStream stream = new FileStream(FileName, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        workbook.Write(stream);
                    }

                }

            }
            catch (Exception ex)
            {
                if (Chap == "CHAP" || Chap == "PDF")
                    Center.ChapEmpty(_hostingEnvironment.WebRootPath, Request.Cookies["Chap.Aparteman.ir"], ".pdf");
                if (Chap == "XLS")
                    Center.ChapEmpty(_hostingEnvironment.WebRootPath, Request.Cookies["Chap.Aparteman.ir"], ".xlsx");

                await DBS.LogErrorSaveAsync(ProcName, Params, Center.GetUserId(Request.Cookies["User.Aparteman.ir"]), FormId, MethodBase.GetCurrentMethod().Name, ex.Message);
            }
            return Content("OK");
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
        {//لیست ردیف های سند
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
        public async Task<IActionResult> OnPostUpdateDaryaft(IFormFile FileUpload, string ID, string OldFile, string Amount, string DateAction, string AccountBes, string TafziliBes, string AccountBed, string TafziliBed, string Sharh)
        {// سند ثبت درآمد/ دریافت
            try
            {
                if (!await DBS.CheckToken(Center.TrimTxt(Request.Cookies["Aparteman.ir"]), userInfo))
                    return RedirectToPage("/LoginFast");

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
                    {"@FinancialEventId", ID },
                    { "@UserId", userInfo.Id },
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
                // اگر ثبت سند اوکی بود و پیوست داشت، پیوست هم آپلود شود
                if (FileUpload != null & SabtDaryaft["success"].ToString().ToUpper() == "OK")
                {
                    string ext = Path.GetExtension(FileUpload.FileName).ToUpperInvariant();

                    if (ext is ".JPG" or ".JPEG" or ".PNG" or ".BMP" or ".PDF")
                    {
                        string folder = Path.Combine(_hostingEnvironment.WebRootPath, "Uploads", "VoucherAttach");

                        // نام فایل براساس کد دریافتی از ذخیره مشخصات ساخته میشود
                        Random random = new Random();
                        int Scr = random.Next(1000, 9999);

                        // 🟡 حذف فایل قدیمی در صورت وجود
                        OldFile = OldFile?.Trim() ?? "temp";
                        string filePath = Path.Combine(folder, OldFile);
                        Center.DeleteFileIfExists(filePath);
                        // حذف پیوست قدیمی
                        if (OldFile != "temp")
                        {
                            Params = new Dictionary<string, object>
                            {
                                { "@FinancialEventId", SabtDaryaft["FinancialEventId"].ToString() },
                            };
                            ProcName = "dbo.FinancialEventAttachment_Delete";
                            await DBS.RunCommandAsync(ProcName, Params);
                        }

                        string fileName = $"V_{SabtDaryaft["FinancialEventId"].ToString()}_{Scr}{ext}";

                        // ثبت پیوست
                        Params = new Dictionary<string, object>
                        {
                            { "@UserId", userInfo.Id },
                            { "@Ext", ext },
                            { "@NameFile", fileName },
                            { "@FinancialEventId", SabtDaryaft["FinancialEventId"].ToString() },
                        };
                        ProcName = "dbo.FinancialEventAttachment_Save";
                        await DBS.RunCommandAsync(ProcName, Params);

                        filePath = Path.Combine(folder, fileName);
                        Center.DeleteFileIfExists(filePath);

                        // تصاویر تغییر سایز داده شوند
                        if (ext is ".JPG" or ".JPEG" or ".PNG" or ".BMP")
                        {
                            // ذخیره موقت
                            string TempFile = Path.Combine(folder, "temp");
                            Center.DeleteFileIfExists(TempFile);
                            using (var stream = new FileStream(TempFile, FileMode.Create))
                            {
                                FileUpload.CopyTo(stream);
                            }

                            // تغییر سایز تصویر
                            ImageResizer _Img = new ImageResizer(800, 800, ext);
                            _Img.Resize(TempFile, filePath);
                        }
                        else
                        {// سایر فایلها
                         // 🟢 ذخیره فایل روی دیسک
                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                FileUpload.CopyTo(stream);
                            }
                        }
                    }

                }

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
                ViewData["ComText"] = Center.CommandText(ProcName, Params);
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
        public async Task<IActionResult> OnGetChap_RepListDaryaft(string ComText, string Chap, string OnvanChap)
        {
            try
            {
                var ReportData = await DBS.GetReportDataAsync(ComText);

                OnvanChap = (Center.TrimTxt(OnvanChap) == "") ? "گزارش دریافت ها" : OnvanChap;
                string webRootPath = _hostingEnvironment.WebRootPath;
                string FileName = System.IO.Path.Combine(webRootPath, "Uploads", Request.Cookies["Chap.Aparteman.ir"]);

                if (Chap == "CHAP" || Chap == "PDF")
                {
                    FileName += ".pdf";
                    Center.DeleteFileIfExists(FileName);

                    // code in your main method
                    Document.Create(container =>
                    {
                        container.Page(page =>
                        {
                            page.Size(PageSizes.A4);
                            page.Margin(1, Unit.Centimetre);
                            page.PageColor(Colors.White);
                            page.DefaultTextStyle(x => x.FontSize(10));
                            page.ContentFromRightToLeft();

                            static IContainer DefaultCellStyle(IContainer container, string backgroundColor = "")
                            {
                                return container
                                  .Border(1)
                                  .BorderColor(Colors.Grey.Lighten1)
                                  .Background(!string.IsNullOrEmpty(backgroundColor) ? backgroundColor : Colors.White)
                                  .PaddingVertical(7)
                                  .PaddingHorizontal(3);
                            }
                            var RJ = 0;

                            page.Header()
                                .Text(OnvanChap)
                                .SemiBold().FontSize(14).FontColor(Colors.Blue.Medium);


                            page.Content()
                                .PaddingVertical(1, Unit.Centimetre)
                                .Border(1)
                                .Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn(7);
                                        columns.RelativeColumn(10);
                                        columns.RelativeColumn(10);
                                        columns.RelativeColumn(10);
                                        columns.RelativeColumn(10);
                                    });
                                    table.Header(header =>
                                    {
                                        header.Cell().Element(CellStyle).AlignCenter().Text("#").FontSize(11).SemiBold();
                                        header.Cell().Element(CellStyle).Text("تاریخ").FontSize(11).SemiBold();
                                        header.Cell().Element(CellStyle).Text("مبلغ").FontSize(11).SemiBold();
                                        header.Cell().Element(CellStyle).Text("بابت/موضوع").FontSize(11).SemiBold();
                                        header.Cell().Element(CellStyle).Text("واریز به حساب").FontSize(11).SemiBold();
                                        header.Cell().Element(CellStyle).Text("شرح").FontSize(11).SemiBold();


                                        // you can extend existing styles by creating additional methods
                                        IContainer CellStyle(IContainer container) => DefaultCellStyle(container, Colors.Grey.Lighten3);
                                    });

                                    foreach (DataRow item in ReportData.Rows)
                                    {
                                        RJ++;
                                        table.Cell().Element(CellStyle).Text(RJ.ToString());
                                        table.Cell().Element(CellStyle).Text(item["EventDate"].ToString());
                                        table.Cell().Element(CellStyle).Text(item["Amount"].ToComma());
                                        table.Cell().Element(CellStyle).Text(item["AccountBes"].ToString());
                                        table.Cell().Element(CellStyle).Text(item["AccountBed"].ToString());
                                        table.Cell().Element(CellStyle).Text(item["Sharh"].ToString());
                                    }

                                    IContainer CellStyle(IContainer container) => DefaultCellStyle(container).ShowOnce();
                                });

                            page.Footer()
                                .AlignCenter()
                                .Text(x =>
                                {
                                    x.Span("صفحه");
                                    x.CurrentPageNumber();
                                });
                        });
                    })
                    .GeneratePdf(FileName);

                }
                if (Chap == "XLS")
                {
                    FileName += ".xlsx";
                    Center.DeleteFileIfExists(FileName);

                    string sheetName = "لیست دریافت ها";
                    // فایلی از نوع xlsx
                    IWorkbook workbook = new XSSFWorkbook();
                    //ایجاد شیت در فایل اکسل
                    ISheet sheet = workbook.CreateSheet(sheetName);
                    sheet.IsRightToLeft = true;
                    // ایجاد عنصر سطر
                    IRow row = sheet.CreateRow(1);

                    IFont font = workbook.CreateFont();
                    font.IsBold = true;
                    font.FontHeightInPoints = 14;
                    ICellStyle boldStyle = workbook.CreateCellStyle();
                    boldStyle.SetFont(font);

                    int _Row = 1;
                    sheet.AddMergedRegion(new CellRangeAddress(_Row, _Row + 1, 1, 10));
                    row = sheet.CreateRow(_Row);
                    Center.CellAddData(row, 1, OnvanChap, CellType.String); ;

                    _Row += 3;
                    row = sheet.CreateRow(_Row);
                    Center.CellAddData(row, 1, "تاریخ", CellType.String);
                    Center.CellAddData(row, 2, "شماره سند", CellType.String);
                    Center.CellAddData(row, 3, "مبلغ", CellType.String);
                    Center.CellAddData(row, 4, "بابت/موضوع", CellType.String);
                    Center.CellAddData(row, 5, "واریز به حساب", CellType.String);
                    Center.CellAddData(row, 6, "شرح", CellType.String);
                    Center.CellAddData(row, 7, "ویرایشگر", CellType.String);
                    Center.CellAddData(row, 8, "زمان ویرایش", CellType.String);

                    foreach (DataRow item in ReportData.Rows)
                    {
                        _Row += 1;
                        row = sheet.CreateRow(_Row);
                        // پرکردن سلولها
                        Center.CellAddData(row, 1, item["EventDate"].ToString(), CellType.Numeric);
                        Center.CellAddData(row, 2, item["FinancialNum"].ToString(), CellType.String);
                        Center.CellAddData(row, 3, item["Amount"].ToString(), CellType.String);
                        Center.CellAddData(row, 4, item["AccountBes"].ToString(), CellType.String);
                        Center.CellAddData(row, 5, item["AccountBed"].ToString(), CellType.String);
                        Center.CellAddData(row, 6, item["Sharh"].ToString(), CellType.String);
                        Center.CellAddData(row, 7, item["EditorName"].ToString(), CellType.String);
                        Center.CellAddData(row, 8, item["TimeEdit"].ToString(), CellType.String);
                    }

                    // سایزبندی 20 ستون در شیت
                    for (int i = 0; i <= 20; i++) sheet.AutoSizeColumn(i, true);

                    using (FileStream stream = new FileStream(FileName, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        workbook.Write(stream);
                    }

                }

            }
            catch (Exception ex)
            {
                if (Chap == "CHAP" || Chap == "PDF")
                    Center.ChapEmpty(_hostingEnvironment.WebRootPath, Request.Cookies["Chap.Aparteman.ir"], ".pdf");
                if (Chap == "XLS")
                    Center.ChapEmpty(_hostingEnvironment.WebRootPath, Request.Cookies["Chap.Aparteman.ir"], ".xlsx");

                await DBS.LogErrorSaveAsync(ProcName, Params, Center.GetUserId(Request.Cookies["User.Aparteman.ir"]), FormId, MethodBase.GetCurrentMethod().Name, ex.Message);
            }
            return Content("OK");
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
        public async Task<IActionResult> OnPostUpdatePardakht(IFormFile FileUpload, string ID, string OldFile, string Amount, string DateAction, string AccountBes, string TafziliBes, string AccountBed, string TafziliBed, string Sharh)
        {// سند ثبت پرداخت
            try
            {
                if (!await DBS.CheckToken(Center.TrimTxt(Request.Cookies["Aparteman.ir"]), userInfo))
                    return RedirectToPage("/LoginFast");

                Amount = Center.CommaClear(Amount);
                if (Amount == "0")
                {
                    ViewData["Switch"] = "MSG";
                    ViewData["MSG"] = "مبلغ پرداختی را بنویسید";

                    return new PartialViewResult
                    {
                        ViewName = "_CommonView",
                        ViewData = this.ViewData
                    };
                }

                // ثبت سند حسابداری پرداخت
                Params = new Dictionary<string, object>
                {
                    {"@FinancialEventId", ID },
                    { "@UserId", userInfo.Id },
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

                // اگر ثبت سند اوکی بود و پیوست داشت، پیوست هم آپلود شود
                if (FileUpload != null & SabtPardakht["success"].ToString().ToUpper() == "OK")
                {
                    string ext = Path.GetExtension(FileUpload.FileName).ToUpperInvariant();

                    if (ext is ".JPG" or ".JPEG" or ".PNG" or ".BMP" or ".PDF")
                    {
                        string folder = Path.Combine(_hostingEnvironment.WebRootPath, "Uploads", "VoucherAttach");

                        // نام فایل براساس کد دریافتی از ذخیره مشخصات ساخته میشود
                        Random random = new Random();
                        int Scr = random.Next(1000, 9999);

                        // 🟡 حذف فایل قدیمی در صورت وجود
                        OldFile = OldFile?.Trim() ?? "temp";
                        string filePath = Path.Combine(folder, OldFile);
                        Center.DeleteFileIfExists(filePath);
                        // حذف پیوست قدیمی
                        if (OldFile != "temp")
                        {
                            Params = new Dictionary<string, object>
                            {
                                { "@FinancialEventId", SabtPardakht["FinancialEventId"].ToString() },
                            };
                            ProcName = "dbo.FinancialEventAttachment_Delete";
                            await DBS.RunCommandAsync(ProcName, Params);
                        }


                        string fileName = $"V_{SabtPardakht["FinancialEventId"].ToString()}_{Scr}{ext}";
                        // ثبت پیوست
                        Params = new Dictionary<string, object>
                        {
                            { "@UserId", userInfo.Id },
                            { "@Ext", ext },
                            { "@NameFile", fileName },
                            { "@FinancialEventId", SabtPardakht["FinancialEventId"].ToString() },
                        };
                        ProcName = "dbo.FinancialEventAttachment_Save";
                        await DBS.RunCommandAsync(ProcName, Params);

                        filePath = Path.Combine(folder, fileName);
                        Center.DeleteFileIfExists(filePath);

                        // تصاویر تغییر سایز داده شوند
                        if (ext is ".JPG" or ".JPEG" or ".PNG" or ".BMP")
                        {
                            // ذخیره موقت
                            string TempFile = Path.Combine(folder, "temp");
                            Center.DeleteFileIfExists(TempFile);
                            using (var stream = new FileStream(TempFile, FileMode.Create))
                            {
                                FileUpload.CopyTo(stream);
                            }

                            // تغییر سایز تصویر
                            ImageResizer _Img = new ImageResizer(800, 800, ext);
                            _Img.Resize(TempFile, filePath);
                        }
                        else
                        {// سایر فایلها
                         // 🟢 ذخیره فایل روی دیسک
                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                FileUpload.CopyTo(stream);
                            }
                        }
                    }

                }

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
                ListData = await DBS.GetReportAsync(ProcName, Params);
                ViewData["ComText"] = Center.CommandText(ProcName, Params);
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

        public async Task<IActionResult> OnGetChap_RepListPardakht(string ComText, string Chap, string OnvanChap)
        {
            try
            {
                var ReportData = await DBS.GetReportDataAsync(ComText);

                OnvanChap = (Center.TrimTxt(OnvanChap) == "") ? "گزارش پرداخت ها" : OnvanChap;
                string webRootPath = _hostingEnvironment.WebRootPath;
                string FileName = System.IO.Path.Combine(webRootPath, "Uploads", Request.Cookies["Chap.Aparteman.ir"]);

                if (Chap == "CHAP" || Chap == "PDF")
                {
                    FileName += ".pdf";
                    Center.DeleteFileIfExists(FileName);

                    // code in your main method
                    Document.Create(container =>
                    {
                        container.Page(page =>
                        {
                            page.Size(PageSizes.A4);
                            page.Margin(1, Unit.Centimetre);
                            page.PageColor(Colors.White);
                            page.DefaultTextStyle(x => x.FontSize(10));
                            page.ContentFromRightToLeft();

                            static IContainer DefaultCellStyle(IContainer container, string backgroundColor = "")
                            {
                                return container
                                  .Border(1)
                                  .BorderColor(Colors.Grey.Lighten1)
                                  .Background(!string.IsNullOrEmpty(backgroundColor) ? backgroundColor : Colors.White)
                                  .PaddingVertical(7)
                                  .PaddingHorizontal(3);
                            }
                            var RJ = 0;

                            page.Header()
                                .Text(OnvanChap)
                                .SemiBold().FontSize(14).FontColor(Colors.Blue.Medium);


                            page.Content()
                                .PaddingVertical(1, Unit.Centimetre)
                                .Border(1)
                                .Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn(7);
                                        columns.RelativeColumn(10);
                                        columns.RelativeColumn(10);
                                        columns.RelativeColumn(10);
                                        columns.RelativeColumn(10);
                                    });
                                    table.Header(header =>
                                    {
                                        header.Cell().Element(CellStyle).AlignCenter().Text("#").FontSize(11).SemiBold();
                                        header.Cell().Element(CellStyle).Text("تاریخ").FontSize(11).SemiBold();
                                        header.Cell().Element(CellStyle).Text("مبلغ").FontSize(11).SemiBold();
                                        header.Cell().Element(CellStyle).Text("بابت/موضوع").FontSize(11).SemiBold();
                                        header.Cell().Element(CellStyle).Text("از حساب").FontSize(11).SemiBold();
                                        header.Cell().Element(CellStyle).Text("شرح").FontSize(11).SemiBold();


                                        // you can extend existing styles by creating additional methods
                                        IContainer CellStyle(IContainer container) => DefaultCellStyle(container, Colors.Grey.Lighten3);
                                    });

                                    foreach (DataRow item in ReportData.Rows)
                                    {
                                        RJ++;
                                        table.Cell().Element(CellStyle).Text(RJ.ToString());
                                        table.Cell().Element(CellStyle).Text(item["EventDate"].ToString());
                                        table.Cell().Element(CellStyle).Text(item["Amount"].ToComma());
                                        table.Cell().Element(CellStyle).Text(item["AccountBed"].ToString());
                                        table.Cell().Element(CellStyle).Text(item["AccountBes"].ToString());
                                        table.Cell().Element(CellStyle).Text(item["Sharh"].ToString());
                                    }

                                    IContainer CellStyle(IContainer container) => DefaultCellStyle(container).ShowOnce();
                                });
        
                            page.Footer()
                                .AlignCenter()
                                .Text(x =>
                                {
                                    x.Span("صفحه");
                                    x.CurrentPageNumber();
                                });
                        });
                    })
                    .GeneratePdf(FileName);

                }
                if (Chap == "XLS")
                {
                    FileName += ".xlsx";
                    Center.DeleteFileIfExists(FileName);

                    string sheetName = "لیست پرداخت ها";
                    // فایلی از نوع xlsx
                    IWorkbook workbook = new XSSFWorkbook();
                    //ایجاد شیت در فایل اکسل
                    ISheet sheet = workbook.CreateSheet(sheetName);
                    sheet.IsRightToLeft = true;
                    // ایجاد عنصر سطر
                    IRow row = sheet.CreateRow(1);

                    IFont font = workbook.CreateFont();
                    font.IsBold = true;
                    font.FontHeightInPoints = 14;
                    ICellStyle boldStyle = workbook.CreateCellStyle();
                    boldStyle.SetFont(font);

                    int _Row = 1;
                    sheet.AddMergedRegion(new CellRangeAddress(_Row, _Row + 1, 1, 10));
                    row = sheet.CreateRow(_Row);
                    Center.CellAddData(row, 1, OnvanChap, CellType.String); ;

                    _Row += 3;
                    row = sheet.CreateRow(_Row);
                    Center.CellAddData(row, 1, "تاریخ", CellType.String);
                    Center.CellAddData(row, 2, "شماره سند", CellType.String);
                    Center.CellAddData(row, 3, "مبلغ", CellType.String);
                    Center.CellAddData(row, 4, "بابت/موضوع", CellType.String);
                    Center.CellAddData(row, 5, "از حساب", CellType.String);
                    Center.CellAddData(row, 6, "شرح", CellType.String);
                    Center.CellAddData(row, 7, "ویرایشگر", CellType.String);
                    Center.CellAddData(row, 8, "زمان ویرایش", CellType.String);

                    foreach (DataRow item in ReportData.Rows)
                    {
                        _Row += 1;
                        row = sheet.CreateRow(_Row);
                        // پرکردن سلولها
                        Center.CellAddData(row, 1, item["EventDate"].ToString(), CellType.Numeric);
                        Center.CellAddData(row, 2, item["FinancialNum"].ToString(), CellType.String);
                        Center.CellAddData(row, 3, item["Amount"].ToString(), CellType.String);
                        Center.CellAddData(row, 4, item["AccountBed"].ToString(), CellType.String);
                        Center.CellAddData(row, 5, item["AccountBes"].ToString(), CellType.String);
                        Center.CellAddData(row, 6, item["Sharh"].ToString(), CellType.String);
                        Center.CellAddData(row, 7, item["EditorName"].ToString(), CellType.String);
                        Center.CellAddData(row, 8, item["TimeEdit"].ToString(), CellType.String);
                    }

                    // سایزبندی 20 ستون در شیت
                    for (int i = 0; i <= 20; i++) sheet.AutoSizeColumn(i, true);

                    using (FileStream stream = new FileStream(FileName, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        workbook.Write(stream);
                    }

                }

            }
            catch (Exception ex)
            {
                if (Chap == "CHAP" || Chap == "PDF")
                    Center.ChapEmpty(_hostingEnvironment.WebRootPath, Request.Cookies["Chap.Aparteman.ir"], ".pdf");
                if (Chap == "XLS")
                    Center.ChapEmpty(_hostingEnvironment.WebRootPath, Request.Cookies["Chap.Aparteman.ir"], ".xlsx");

                await DBS.LogErrorSaveAsync(ProcName, Params, Center.GetUserId(Request.Cookies["User.Aparteman.ir"]), FormId, MethodBase.GetCurrentMethod().Name, ex.Message);
            }
            return Content("OK");
        }



        public async Task<IActionResult> OnGetFormSabtTransfer(string ID = "0")
        {// فرم انتقال بین حسابها
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
                    { "@AccountTypeCode" , 1 } ,
                };
                ProcName = "dbo.Accounts_ListActive";
                ListAccount = await DBS.GetReportAsync(ProcName, Params);

                ViewData["Today"] = await DBS.Emruzasync();
                ViewData["Switch"] = "FormSabtTransfer";
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
        public async Task<IActionResult> OnPostUpdateTransfer(IFormFile FileUpload, string ID, string OldFile, string Amount, string DateAction, string AccountBes, string TafziliBes, string AccountBed, string TafziliBed, string Sharh)
        {// سند ثبت انتقال
            try
            {
                if (!await DBS.CheckToken(Center.TrimTxt(Request.Cookies["Aparteman.ir"]), userInfo))
                    return RedirectToPage("/LoginFast");

                Amount = Center.CommaClear(Amount);
                if (Amount == "0")
                {
                    ViewData["Switch"] = "MSG";
                    ViewData["MSG"] = "مبلغ انتقالی را بنویسید";

                    return new PartialViewResult
                    {
                        ViewName = "_CommonView",
                        ViewData = this.ViewData
                    };
                }

                // ثبت سند حسابداری انتقال
                Params = new Dictionary<string, object>
                {
                    {"@FinancialEventId", ID },
                    { "@UserId", userInfo.Id },
                    { "@Amount", Amount },
                    { "@DateAction", DateAction },
                    { "@AccountBes", AccountBes },
                    { "@TafziliBes", TafziliBes },
                    { "@AccountBed", AccountBed },
                    { "@TafziliBed", TafziliBed },
                    { "@Sharh", Sharh },
                };

                ProcName = "dbo.FinanceSabtTransfer";
                var SabtTransfer = await DBS.GetReportRowAsync(ProcName, Params);

                // اگر ثبت سند اوکی بود و پیوست داشت، پیوست هم آپلود شود
                if (FileUpload != null & SabtTransfer["success"].ToString().ToUpper() == "OK")
                {
                    string ext = Path.GetExtension(FileUpload.FileName).ToUpperInvariant();

                    if (ext is ".JPG" or ".JPEG" or ".PNG" or ".BMP" or ".PDF")
                    {
                        string folder = Path.Combine(_hostingEnvironment.WebRootPath, "Uploads", "VoucherAttach");

                        // نام فایل براساس کد دریافتی از ذخیره مشخصات ساخته میشود
                        Random random = new Random();
                        int Scr = random.Next(1000, 9999);

                        // 🟡 حذف فایل قدیمی در صورت وجود
                        OldFile = OldFile?.Trim() ?? "temp";
                        string filePath = Path.Combine(folder, OldFile);
                        Center.DeleteFileIfExists(filePath);
                        // حذف پیوست قدیمی
                        if (OldFile != "temp")
                        {
                            Params = new Dictionary<string, object>
                            {
                                { "@FinancialEventId", SabtTransfer["FinancialEventId"].ToString() },
                            };
                            ProcName = "dbo.FinancialEventAttachment_Delete";
                            await DBS.RunCommandAsync(ProcName, Params);
                        }


                        string fileName = $"V_{SabtTransfer["FinancialEventId"].ToString()}_{Scr}{ext}";
                        // ثبت پیوست
                        Params = new Dictionary<string, object>
                        {
                            { "@UserId", userInfo.Id },
                            { "@Ext", ext },
                            { "@NameFile", fileName },
                            { "@FinancialEventId", SabtTransfer["FinancialEventId"].ToString() },
                        };
                        ProcName = "dbo.FinancialEventAttachment_Save";
                        await DBS.RunCommandAsync(ProcName, Params);

                        filePath = Path.Combine(folder, fileName);
                        Center.DeleteFileIfExists(filePath);

                        // تصاویر تغییر سایز داده شوند
                        if (ext is ".JPG" or ".JPEG" or ".PNG" or ".BMP")
                        {
                            // ذخیره موقت
                            string TempFile = Path.Combine(folder, "temp");
                            Center.DeleteFileIfExists(TempFile);
                            using (var stream = new FileStream(TempFile, FileMode.Create))
                            {
                                FileUpload.CopyTo(stream);
                            }

                            // تغییر سایز تصویر
                            ImageResizer _Img = new ImageResizer(800, 800, ext);
                            _Img.Resize(TempFile, filePath);
                        }
                        else
                        {// سایر فایلها
                         // 🟢 ذخیره فایل روی دیسک
                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                FileUpload.CopyTo(stream);
                            }
                        }
                    }

                }

                return new JsonResult(SabtTransfer);
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
        public async Task<IActionResult> OnGetRepListTransfer(string FromDate, string ToDate, string AccountId, string TafziliId
                , string Sharh, string PayerCode, string Sort)
        {//لیست انتقال ها
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
                ProcName = "dbo.FinancialEvents_ReportTransfer";
                ListData = await DBS.GetReportAsync(ProcName, Params);
                ViewData["ComText"] = Center.CommandText(ProcName, Params);
                if (ListData.Rows.Count == 0)
                {
                    ViewData["Switch"] = "MSG";
                    ViewData["MSG"] = "سند انتقال با توجه به شرایط فوق پیدا نشد. \n شرایط گزارش گیری را تغییر بدهید و دوباره گزارش بگیرید";

                    return new PartialViewResult
                    {
                        ViewName = "_CommonView",
                        ViewData = this.ViewData
                    };

                }

                ViewData["Switch"] = "RepListTransfer";
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

        public async Task<IActionResult> OnGetChap_RepListTransfer(string ComText, string Chap, string OnvanChap)
        {
            try
            {
                var ReportData = await DBS.GetReportDataAsync(ComText);

                OnvanChap = (Center.TrimTxt(OnvanChap) == "") ? "گزارش انتقال بین حسابها" : OnvanChap;
                string webRootPath = _hostingEnvironment.WebRootPath;
                string FileName = System.IO.Path.Combine(webRootPath, "Uploads" , Request.Cookies["Chap.Aparteman.ir"]);

                if (Chap == "CHAP" || Chap == "PDF")
                {
                    FileName += ".pdf";
                    Center.DeleteFileIfExists(FileName);

                    // code in your main method
                    Document.Create(container =>
                    {
                        container.Page(page =>
                        {
                            page.Size(PageSizes.A4);
                            page.Margin(1, Unit.Centimetre);
                            page.PageColor(Colors.White);
                            page.DefaultTextStyle(x => x.FontSize(10));
                            page.ContentFromRightToLeft();

                            static IContainer DefaultCellStyle(IContainer container, string backgroundColor = "")
                            {
                                return container
                                  .Border(1)
                                  .BorderColor(Colors.Grey.Lighten1)
                                  .Background(!string.IsNullOrEmpty(backgroundColor) ? backgroundColor : Colors.White)
                                  .PaddingVertical(7)
                                  .PaddingHorizontal(3);
                            }
                            var RJ = 0;

                            page.Header()
                                .Text(OnvanChap)
                                .SemiBold().FontSize(14).FontColor(Colors.Blue.Medium);


                            page.Content()
                                .PaddingVertical(1, Unit.Centimetre)
                                .Border(1)
                                .Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn(7);
                                        columns.RelativeColumn(10);
                                        columns.RelativeColumn(10);
                                        columns.RelativeColumn(10);
                                        columns.RelativeColumn(10);
                                    });
                                    table.Header(header =>
                                    {
                                        header.Cell().Element(CellStyle).AlignCenter().Text("#").FontSize(11).SemiBold();
                                        header.Cell().Element(CellStyle).Text("تاریخ").FontSize(11).SemiBold();
                                        header.Cell().Element(CellStyle).Text("مبلغ").FontSize(11).SemiBold();
                                        header.Cell().Element(CellStyle).Text("از حساب").FontSize(11).SemiBold();
                                        header.Cell().Element(CellStyle).Text("به حساب").FontSize(11).SemiBold();
                                        header.Cell().Element(CellStyle).Text("شرح").FontSize(11).SemiBold();


                                        // you can extend existing styles by creating additional methods
                                        IContainer CellStyle(IContainer container) => DefaultCellStyle(container, Colors.Grey.Lighten3);
                                    });

                                    foreach (DataRow item in ReportData.Rows)
                                    {
                                        RJ++;
                                        table.Cell().Element(CellStyle).Text(RJ.ToString());
                                        table.Cell().Element(CellStyle).Text(item["EventDate"].ToString());
                                        table.Cell().Element(CellStyle).Text(item["Amount"].ToComma());
                                        table.Cell().Element(CellStyle).Text(item["AccountBes"].ToString());
                                        table.Cell().Element(CellStyle).Text(item["AccountBed"].ToString());
                                        table.Cell().Element(CellStyle).Text(item["Sharh"].ToString());
                                    }

                                    IContainer CellStyle(IContainer container) => DefaultCellStyle(container).ShowOnce();
                                });

                            page.Footer()
                                .AlignCenter()
                                .Text(x =>
                                {
                                    x.Span("صفحه");
                                    x.CurrentPageNumber();
                                });
                        });
                    })
                    .GeneratePdf(FileName);

                }
                if (Chap == "XLS")
                {
                    FileName += ".xlsx";
                    Center.DeleteFileIfExists(FileName);

                    string sheetName = "لیست انتقال های بین حسابها";
                    // فایلی از نوع xlsx
                    IWorkbook workbook = new XSSFWorkbook();
                    //ایجاد شیت در فایل اکسل
                    ISheet sheet = workbook.CreateSheet(sheetName);
                    sheet.IsRightToLeft = true;
                    // ایجاد عنصر سطر
                    IRow row = sheet.CreateRow(1);

                    IFont font = workbook.CreateFont();
                    font.IsBold = true;
                    font.FontHeightInPoints = 14;
                    ICellStyle boldStyle = workbook.CreateCellStyle();
                    boldStyle.SetFont(font);

                    int _Row = 1;
                    sheet.AddMergedRegion(new CellRangeAddress(_Row, _Row + 1, 1, 10));
                    row = sheet.CreateRow(_Row);
                    Center.CellAddData(row, 1, OnvanChap, CellType.String); ;

                    _Row += 3;
                    row = sheet.CreateRow(_Row);
                    Center.CellAddData(row, 1, "تاریخ", CellType.String);
                    Center.CellAddData(row, 2, "شماره سند", CellType.String);
                    Center.CellAddData(row, 3, "مبلغ", CellType.String);
                    Center.CellAddData(row, 4, "از حساب", CellType.String);
                    Center.CellAddData(row, 5, "به حساب", CellType.String);
                    Center.CellAddData(row, 6, "شرح", CellType.String);
                    Center.CellAddData(row, 7, "ویرایشگر", CellType.String);
                    Center.CellAddData(row, 8, "زمان ویرایش", CellType.String);

                    foreach (DataRow item in ReportData.Rows)
                    {
                        _Row += 1;
                        row = sheet.CreateRow(_Row);
                        // پرکردن سلولها
                        Center.CellAddData(row, 1, item["EventDate"].ToString(), CellType.Numeric);
                        Center.CellAddData(row, 2, item["FinancialNum"].ToString(), CellType.String);
                        Center.CellAddData(row, 3, item["Amount"].ToString(), CellType.String);
                        Center.CellAddData(row, 4, item["AccountBes"].ToString(), CellType.String);
                        Center.CellAddData(row, 5, item["AccountBed"].ToString(), CellType.String);
                        Center.CellAddData(row, 6, item["Sharh"].ToString(), CellType.String);
                        Center.CellAddData(row, 7, item["EditorName"].ToString(), CellType.String);
                        Center.CellAddData(row, 8, item["TimeEdit"].ToString(), CellType.String);
                    }

                    // سایزبندی 20 ستون در شیت
                    for (int i = 0; i <= 20; i++) sheet.AutoSizeColumn(i, true);

                    using (FileStream stream = new FileStream(FileName, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        workbook.Write(stream);
                    }

                }

            }
            catch (Exception ex)
            {
                if (Chap == "CHAP" || Chap == "PDF")
                    Center.ChapEmpty(_hostingEnvironment.WebRootPath, Request.Cookies["Chap.Aparteman.ir"], ".pdf");
                if (Chap == "XLS")
                    Center.ChapEmpty(_hostingEnvironment.WebRootPath, Request.Cookies["Chap.Aparteman.ir"], ".xlsx");

                await DBS.LogErrorSaveAsync(ProcName, Params, Center.GetUserId(Request.Cookies["User.Aparteman.ir"]), FormId, MethodBase.GetCurrentMethod().Name, ex.Message);
            }
            return Content("OK");
        }

    
    }
}
