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
using System.IO;
namespace Aparteman.Pages.dashboard
{
    public class NoticesModel : PageModel
    {
        private string ProcName = ""; private Dictionary<string, object> Params;
        private string VU = "Notices_View";
        public Int32 FormId = 20;

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
        public NoticesModel(IWebHostEnvironment hostingEnvironment)
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

                return Page();
            }
            catch (Exception ex)
            {
                ViewData["ERRComm"] = await DBS.LogErrorSaveAsync(ProcName, Params, Center.GetUserId(Request.Cookies["User.Aparteman.ir"]), FormId, MethodBase.GetCurrentMethod().Name, ex.Message);
                return RedirectToPage("/Other/Khata", new { returnUrl = Url.PageLink() });
            }
        }

        public async Task<IActionResult> OnGetFormSabtElanat(string ID)
        {// فرم ثبت اعلانات
            try
            {
                ViewData["ID"] = ID;

                if (ID != "0")
                {
                    var UserId = Center.GetUserId(Request.Cookies["User.Aparteman.ir"]);
                    Params = new Dictionary<string, object>
                    {
                        { "@UserId" , UserId } ,
                        { "@ElanatId" , ID } ,
                    };
                    ProcName = "dbo.Elanat_Info";
                    RowData = await DBS.GetReportRowAsync(ProcName, Params);
                }
                else
                    ViewData["Today"] = await DBS.Emruzasync();

                ViewData["Switch"] = "FormSabtElanat";
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
        public async Task<IActionResult> OnPostUpdateElan(string ID, string Onvan, string Matn, string Sath, string FromDate, string ToDate)
        {// ذخیره یک اطلاعیه
            try
            {
                if (!await DBS.CheckToken(Center.TrimTxt(Request.Cookies["Aparteman.ir"]), userInfo))
                    return RedirectToPage("/LoginFast");

                Params = new Dictionary<string, object>
                {
                    { "@ElanatId", ID },
                    { "@Onvan", Onvan },
                    { "@Matn", Matn },
                    { "@Sath", Sath },
                    { "@FromDate", FromDate },
                    { "@ToDate", ToDate },
                    { "@UserId", userInfo.Id },
                };

                ProcName = "dbo.Elanat_Save";

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
        public async Task<IActionResult> OnGetRepListElanat(string FromDate, string ToDate, string Onvan, string Sort)
        {//گزارش اعلانات
            try
            {
                var userId = Center.GetUserId(Request.Cookies["User.Aparteman.ir"]);

                Params = new Dictionary<string, object>
                {
                    { "@UserId", userId },
                    { "@FromDate", FromDate },
                    { "@ToDate", ToDate },
                    { "@Onvan", Onvan },
                    { "@Sort", Sort },
                };
                ProcName = "dbo.Elanat_Report";
                ListData = await DBS.GetReportAsync(ProcName, Params);
                ViewData["ComText"] = Center.CommandText(ProcName, Params);
                if (ListData.Rows.Count == 0)
                {
                    ViewData["Switch"] = "MSG";
                    ViewData["MSG"] = "اطلاعیه ای با توجه به شرایط فوق پیدا نشد. \n شرایط گزارش گیری را تغییر بدهید و دوباره گزارش بگیرید";

                    return new PartialViewResult
                    {
                        ViewName = "_CommonView",
                        ViewData = this.ViewData
                    };

                }

                ViewData["Switch"] = "RepListElanat";
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
        public async Task<IActionResult> OnGetChap_RepListElanat(string ComText, string Chap, string OnvanChap)
        {
            try
            {
                var ReportData = await DBS.GetReportDataAsync(ComText);

                OnvanChap = (Center.TrimTxt(OnvanChap) == "") ? "گزارش اعلانات" : OnvanChap;
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

                    string sheetName = "لیست اعلانات";
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




        public async Task<IActionResult> OnGetMatnElanat(string ID)
        {// فرم ثبت اعلانات
            try
            {
                var UserId = Center.GetUserId(Request.Cookies["User.Aparteman.ir"]);
                Params = new Dictionary<string, object>
                {
                    { "@UserId" , UserId } ,
                    { "@ElanatId" , ID } ,
                };
                ProcName = "dbo.Elanat_Info";
                RowData = await DBS.GetReportRowAsync(ProcName, Params);

                ViewData["Switch"] = "MatnElanat";
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
