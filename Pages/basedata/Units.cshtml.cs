using Aparteman.Models;
using Aparteman.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
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

        private IWebHostEnvironment _hostingEnvironment;
        public UnitsModel(IWebHostEnvironment hostingEnvironment)
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
                ViewData["ComText"] = Center.CommandText(ProcName, Params);

                if (ListUnit.Rows.Count == 0)
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
        public async Task<IActionResult> OnGetChap_ListUnits(string ComText, string Chap, string OnvanChap)
        {
            try
            {
                var ReportData = await DBS.GetReportDataAsync(ComText);

                OnvanChap = (Center.TrimTxt(OnvanChap) == "") ? "گزارش واحدها" : OnvanChap;
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
                                        columns.RelativeColumn(10);
                                        columns.RelativeColumn(4);
                                        columns.RelativeColumn(5);
                                        columns.RelativeColumn(5);
                                        columns.RelativeColumn(5);
                                        columns.RelativeColumn(5);
                                        columns.RelativeColumn(6);
                                    });
                                    table.Header(header =>
                                    {
                                        header.Cell().Element(CellStyle).AlignCenter().Text("#").FontSize(11).SemiBold();
                                        header.Cell().Element(CellStyle).Text("واحد").FontSize(11).SemiBold();
                                        header.Cell().Element(CellStyle).Text("مساحت").FontSize(11).SemiBold();
                                        header.Cell().Element(CellStyle).Text("نفرات").FontSize(11).SemiBold();
                                        header.Cell().Element(CellStyle).Text("شارژ").FontSize(11).SemiBold();
                                        header.Cell().Element(CellStyle).Text("سهم ها").FontSize(11).SemiBold();
                                        header.Cell().Element(CellStyle).Text("وضعیت").FontSize(11).SemiBold();
                                        header.Cell().Element(CellStyle).Text("مالک/ساکن").FontSize(11).SemiBold();


                                        // you can extend existing styles by creating additional methods
                                        IContainer CellStyle(IContainer container) => DefaultCellStyle(container, Colors.Grey.Lighten3);
                                    });

                                    foreach (DataRow item in ReportData.Rows)
                                    {
                                        RJ++;
                                        table.Cell().Element(CellStyle).Text(RJ.ToString());
                                        table.Cell().Element(CellStyle).Text(item["BuildingCode"].ToString()+" -> " + item["BuildingName"].ToString() + " \n" + item["FloorNumber"].ToString() + " -> " + item["FloorTitle"].ToString() + "\n" + item["UnitNumber"].ToString());
                                        table.Cell().Element(CellStyle).Text(item["Area"].ToComma());
                                        table.Cell().Element(CellStyle).Text("تعداد ساکن: " + item["NqSaken"].ToString() + "\nتعداد شارژ: " + item["NqCharge"].ToString());
                                        if(item["IsActiveMalek"].ToYesNo() == "1" & item["IsActiveSaken"].ToYesNo() == "1") { table.Cell().Element(CellStyle).Text("شارژ مالک \nشارژ ساکن "); }
                                        else
                                        {
                                            if (item["IsActiveMalek"].ToYesNo() == "1" & item["IsActiveSaken"].ToYesNo() == "0") { table.Cell().Element(CellStyle).Text("شارژ مالک"); }
                                            else
                                            {
                                                if (item["IsActiveMalek"].ToYesNo() == "0" & item["IsActiveSaken"].ToYesNo() == "1") { table.Cell().Element(CellStyle).Text("\nشارژ ساکن "); }
                                                else
                                                {
                                                    table.Cell().Element(CellStyle).Text("\n");
                                                }
                                            }
                                        }
                                        table.Cell().Element(CellStyle).Text("اشتراک: " + item["SharePercent"].ToString() + "\nآسانسور: " + item["ZaribAsan"].ToString());
                                        table.Cell().Element(CellStyle).Text(item["SitTitle"].ToString());
                                        table.Cell().Element(CellStyle).Text(item["NameMalek"].ToString() + "\n ->" + item["NameSaken"].ToString());
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

                    string sheetName = "لیست واحدها";
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
                    Center.CellAddData(row, 1, "کد ساختمان", CellType.String);
                    Center.CellAddData(row, 2, "ساختمان", CellType.String);
                    Center.CellAddData(row, 3, "شماره طبقه", CellType.String);
                    Center.CellAddData(row, 4, "نام طبقه", CellType.String);
                    Center.CellAddData(row, 5, "واحد", CellType.String);
                    Center.CellAddData(row, 6, "مساحت", CellType.String);
                    Center.CellAddData(row, 7, "تعداد ساکنین", CellType.String);
                    Center.CellAddData(row, 8, "تعداد شارژ", CellType.String);
                    Center.CellAddData(row, 9, "شارژ مالک", CellType.String);
                    Center.CellAddData(row, 10, "شارژ ساکن", CellType.String);
                    Center.CellAddData(row, 11, "سهم اشتراک", CellType.String);
                    Center.CellAddData(row, 12, "ضریب آسانسور", CellType.String);
                    Center.CellAddData(row, 13, "وضعیت", CellType.String);
                    Center.CellAddData(row, 14, "مالک", CellType.String);
                    Center.CellAddData(row, 15, "ساکن", CellType.String);
                    Center.CellAddData(row, 16, "شومینه", CellType.String);
                    Center.CellAddData(row, 17, "خواب", CellType.String);
                    Center.CellAddData(row, 18, "کنتور آب", CellType.String);
                    Center.CellAddData(row, 19, "کنتور برق", CellType.String);
                    Center.CellAddData(row, 20, "کنتور گاز", CellType.String);
                    Center.CellAddData(row, 21, "شماره پارکینگ", CellType.String);
                    Center.CellAddData(row, 22, "شماره انباری", CellType.String);
                    Center.CellAddData(row, 23, "مساحت انباری", CellType.String);
                    Center.CellAddData(row, 24, "شرح", CellType.String);
                    Center.CellAddData(row, 25, "ویرایشگر", CellType.String);
                    Center.CellAddData(row, 26, "زمان ویرایش", CellType.String);


                    foreach (DataRow item in ReportData.Rows)
                    {
                        _Row += 1;
                        row = sheet.CreateRow(_Row);
                        // پرکردن سلولها
                        Center.CellAddData(row, 1, item["BuildingCode"].ToString(), CellType.String);
                        Center.CellAddData(row, 2, item["BuildingName"].ToString(), CellType.String);
                        Center.CellAddData(row, 3, item["FloorNumber"].ToString(), CellType.Numeric);
                        Center.CellAddData(row, 4, item["FloorTitle"].ToString(), CellType.String);
                        Center.CellAddData(row, 5, item["UnitNumber"].ToString(), CellType.Numeric);
                        Center.CellAddData(row, 6, item["Area"].ToString(), CellType.Numeric);
                        Center.CellAddData(row, 7, item["NqSaken"].ToString(), CellType.Numeric);
                        Center.CellAddData(row, 8, item["NqCharge"].ToString(), CellType.Numeric);
                        if (item["IsActiveMalek"].ToYesNo() == "1")
                            Center.CellAddData(row, 9, "بلی", CellType.String);
                        else
                            Center.CellAddData(row, 9, "خیر", CellType.String);
                        if (item["IsActiveSaken"].ToYesNo() == "1")
                            Center.CellAddData(row, 10, "بلی", CellType.String);
                        else
                            Center.CellAddData(row, 10, "خیر", CellType.String);

                        Center.CellAddData(row, 11, item["SharePercent"].ToString(), CellType.Numeric);
                        Center.CellAddData(row, 12, item["ZaribAsan"].ToString(), CellType.Numeric);
                        Center.CellAddData(row, 13, item["SitTitle"].ToString(), CellType.String);
                        Center.CellAddData(row, 14, item["NameMalek"].ToString(), CellType.String);
                        Center.CellAddData(row, 15, item["NameSaken"].ToString(), CellType.String);
                        if (item["Shomine"].ToYesNo() == "1")
                            Center.CellAddData(row, 16, "بلی", CellType.String);
                        else
                            Center.CellAddData(row, 16, "خیر", CellType.String);

                        Center.CellAddData(row, 17, item["NqBed"].ToString(), CellType.Numeric);
                        Center.CellAddData(row, 18, item["NumberWater"].ToString(), CellType.Numeric);
                        Center.CellAddData(row, 19, item["NumberBarg"].ToString(), CellType.Numeric);
                        Center.CellAddData(row, 20, item["NumBerGaz"].ToString(), CellType.Numeric);
                        Center.CellAddData(row, 21, item["NumberPark"].ToString(), CellType.Numeric);
                        Center.CellAddData(row, 22, item["NumberStor"].ToString(), CellType.Numeric);
                        Center.CellAddData(row, 23, item["AreaStore"].ToString(), CellType.Numeric);
                        Center.CellAddData(row, 24, item["Sharh"].ToString(), CellType.String);
                        Center.CellAddData(row, 25, item["EditorName"].ToString(), CellType.String);
                        Center.CellAddData(row, 26, item["TimeEdit"].ToString(), CellType.String);
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
