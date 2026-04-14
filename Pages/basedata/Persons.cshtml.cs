using Aparteman.Models;
using Aparteman.Services;
using Microsoft.AspNetCore.Components.Forms;
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
    public class PersonsModel : PageModel
    {
        private string ProcName = ""; private Dictionary<string, object> Params;
        private string URL = "Persons";
        private string VU = "Persons_View";
        public Int32 FormId = 4;

        public UserInfo userInfo = new UserInfo();
        public required FormData formData;

        public required DataTable ListComplex;
        public required DataTable ListBuilding;
        public required DataTable ListFloor;
        public required DataTable ListPersons;
        public required DataTable ListUnit;
        public required DataTable ListAsnad;

        public required DataTable ListData;
        public required DataTable ListForms;
        public required DataRow RowData;

        private Microsoft.AspNetCore.Hosting.IWebHostEnvironment _hostingEnvironment;
        public PersonsModel(Microsoft.AspNetCore.Hosting.IWebHostEnvironment hostingEnvironment)
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
                return RedirectToPage("/Other/Khata", new { returnUrl = URL });
            }
        }

        public async Task<IActionResult> OnGetListPersons(string BuildingId, string FloorId, string RoleCode, string Active, string Mobile, string FullName, string NationalCode, string UnitNumber, string Sort)
        {// لیست اشخاص
            try
            {
                var UserId = Center.GetUserId(Request.Cookies["User.Aparteman.ir"]);
                Params = new Dictionary<string, object>
                {
                    { "@UserId", UserId },
                    { "@BuildingId", BuildingId },
                    { "@FloorId", FloorId },
                    { "@UnitNumber", UnitNumber },
                    { "@RoleCode", RoleCode },
                    { "@Active", Active },
                    { "@Mobile", Mobile },
                    { "@FullName", FullName },
                    { "@NationalCode", NationalCode },
                    { "@Sort", Sort },
                };
                ProcName = "Persons_Search";

                ListPersons = await DBS.GetReportAsync(ProcName, Params);
                await DBS.TestProcedure(ProcName, Params);
                ViewData["ComText"] = Center.CommandText(ProcName, Params);

                if (ListPersons.Rows.Count == 0)
                {
                    ViewData["Switch"] = "MSG";
                    ViewData["MSG"] = "فردی با مشخصات فوق پیدا نشد. شرایط جستجو را تغییر بدهید و دوباره گزارش بگیرید";

                    return new PartialViewResult
                    {
                        ViewName = "_CommonView",
                        ViewData = this.ViewData
                    };
                };

                ViewData["Switch"] = "ListPerson";

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
        public async Task<IActionResult> OnGetFormSabtPerson(string ID)
        {// فرم ثبت دیتای اشخاص
            try
            {
                ViewData["ID"] = ID;

                var UserId = Center.GetUserId(Request.Cookies["User.Aparteman.ir"]);
                if (ID != "0")
                {
                    Params = new Dictionary<string, object>
                    {
                        { "@UserId" , UserId } ,
                        { "@PersonId" , ID } ,
                    };
                    ProcName = "Persons_Info";
                    //await DBS.TestProcedure(ProcName, Params);

                    RowData = await DBS.GetReportRowAsync(ProcName, Params);
                }
                ViewData["Switch"] = "FormSabtPerson";
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
        public async Task<IActionResult> OnPostUpdatePerson(string ID, string IsActive, string IsUser, string FullName, string Mobile, string NationalCode
                        , string Email, string Sharh, string BirthDate, string Phone)
        {//ثبت شخص
            try
            {
                if (!await DBS.CheckToken(Center.TrimTxt(Request.Cookies["Aparteman.ir"]), userInfo))
                    return RedirectToPage("/LoginFast");

                Params = new Dictionary<string, object>
                {
                    { "@CurrentUserId", userInfo.Id } ,
                    { "@PersonId", ID },
                    { "@FullName", FullName },
                    { "@NationalCode", NationalCode },
                    { "@Mobile" , Mobile } ,
                    { "@Email" , Email } ,
                    { "@IsActive", IsActive },
                    { "@IsUser", IsUser },
                    { "@BirthDate", BirthDate },
                    { "@Phone", Phone },
                    { "@Sharh", Sharh },
                };


                ProcName = "Persons_Save";
                var Save = await DBS.GetReportRowAsync(ProcName, Params);
                if (ID == "0" & IsUser == "1")
                { // کاربر جدید سیستم است
                    string PersonId = Save["PersonId"].ToString();
                    string PassNew = PasswordService.HashPassword(PersonId);
                    // اصلاح رمز عبور برابر با کد کاربر
                    Params = new Dictionary<string, object>
                    {
                        { "@UserId", userInfo.Id } ,
                        { "@PersonId", PersonId },
                        { "@NewPasswordHash", PassNew },
                    };
                    ProcName = "Users_ResetPasswordByAdmin";
                    await DBS.RunCommandAsync(ProcName, Params);

                }
                return new JsonResult(Save);

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
        public async Task<IActionResult> OnPostDeletePerson(string ID)
        {//حذف شخص
            try
            {
                if (!await DBS.CheckToken(Center.TrimTxt(Request.Cookies["Aparteman.ir"]), userInfo))
                    return RedirectToPage("/LoginFast");

                Params = new Dictionary<string, object>
                {
                    { "@ID", ID },
                };

                ProcName = "Persons_Delete";
                
                return Content(await DBS.GetReportResultAsync(ProcName, Params,"Res"));

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
        public async Task<IActionResult> OnGetChap_ListPerson(string ComText, string Chap, string OnvanChap)
        {
            try
            {
                var ReportData = await DBS.GetReportDataAsync(ComText);

                OnvanChap = (Center.TrimTxt(OnvanChap) == "") ? "گزارش اشخاص" : OnvanChap;
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
                                        columns.RelativeColumn(8);
                                        columns.RelativeColumn(5);
                                        columns.RelativeColumn(5);
                                        columns.RelativeColumn(5);
                                        columns.RelativeColumn(10);
                                    });
                                    table.Header(header =>
                                    {
                                        header.Cell().Element(CellStyle).AlignCenter().Text("#").FontSize(11).SemiBold();
                                        header.Cell().Element(CellStyle).Text("نام").FontSize(11).SemiBold();
                                        header.Cell().Element(CellStyle).Text("تلفن").FontSize(11).SemiBold();
                                        header.Cell().Element(CellStyle).Text("کدملی").FontSize(11).SemiBold();
                                        header.Cell().Element(CellStyle).Text("نقش").FontSize(11).SemiBold();
                                        header.Cell().Element(CellStyle).Text("واحد").FontSize(11).SemiBold();

                                        // you can extend existing styles by creating additional methods
                                        IContainer CellStyle(IContainer container) => DefaultCellStyle(container, Colors.Grey.Lighten3);
                                    });

                                    foreach (DataRow item in ReportData.Rows)
                                    {
                                        RJ++;
                                        table.Cell().Element(CellStyle).Text(RJ.ToString());
                                        table.Cell().Element(CellStyle).Text(item["FullName"].ToString());
                                        table.Cell().Element(CellStyle).Text(item["Mobile"].ToString()+ "\n" + item["Phone"].ToString());
                                        table.Cell().Element(CellStyle).Text(item["NationalCode"].ToString());
                                        table.Cell().Element(CellStyle).Text(item["RoleTitle"].ToString());
                                        table.Cell().Element(CellStyle).Text(item["BuildingCode"].ToString() + " -> " + item["BuildingName"].ToString() + "\n" + item["FloorNumber"].ToString() + " -> " + item["FloorTitle"].ToString() + "\n واحد: " + item["UnitNumber"].ToString());
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

                    string sheetName = "لیست اشخاص";
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
                    Center.CellAddData(row, 1, "نام", CellType.String);
                    Center.CellAddData(row, 2, "شماره همراه", CellType.String);
                    Center.CellAddData(row, 3, "تلفن", CellType.String);
                    Center.CellAddData(row, 4, "کد ملی", CellType.String);
                    Center.CellAddData(row, 5, "ایمیل", CellType.String);
                    Center.CellAddData(row, 6, "نقش", CellType.String);
                    Center.CellAddData(row, 7, "کد ساختمان", CellType.String);
                    Center.CellAddData(row, 8, "نام ساختمان", CellType.String);
                    Center.CellAddData(row, 9, "شماره طبقه", CellType.String);
                    Center.CellAddData(row, 10, "طبقه", CellType.String);
                    Center.CellAddData(row, 11, "واحد", CellType.String);
                    Center.CellAddData(row, 12, "وضعیت فعال", CellType.String);
                    Center.CellAddData(row, 13, "شرح", CellType.String);
                    Center.CellAddData(row, 14, "ویرایشگر", CellType.String);
                    Center.CellAddData(row, 15, "زمان ویرایش", CellType.String);


                    foreach (DataRow item in ReportData.Rows)
                    {
                        _Row += 1;
                        row = sheet.CreateRow(_Row);
                        // پرکردن سلولها
                        Center.CellAddData(row, 1, item["FullName"].ToString(), CellType.String);
                        Center.CellAddData(row, 2, item["Mobile"].ToString(), CellType.String);
                        Center.CellAddData(row, 3, item["Phone"].ToString(), CellType.String);

                        Center.CellAddData(row, 4, item["NationalCode"].ToString(), CellType.Numeric);
                        Center.CellAddData(row, 5, item["Email"].ToString(), CellType.String);
                        Center.CellAddData(row, 6, item["RoleTitle"].ToString(), CellType.String);

                        Center.CellAddData(row, 7, item["BuildingCode"].ToString(), CellType.String);
                        Center.CellAddData(row, 8, item["BuildingName"].ToString(), CellType.String);
                        Center.CellAddData(row, 9, item["FloorNumber"].ToString(), CellType.Numeric);
                        Center.CellAddData(row, 10, item["FloorTitle"].ToString(), CellType.String);
                        Center.CellAddData(row, 11, item["UnitNumber"].ToString(), CellType.Numeric);

                        if (item["IsActive"].ToYesNo() == "1")
                            Center.CellAddData(row, 12, "بلی", CellType.String);
                        else
                            Center.CellAddData(row, 12, "خیر", CellType.String);

                        Center.CellAddData(row, 13, item["Sharh"].ToString(), CellType.String);
                        Center.CellAddData(row, 14, item["EditorName"].ToString(), CellType.String);
                        Center.CellAddData(row, 15, item["TimeEdit"].ToString(), CellType.String);
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



        public async Task<IActionResult> OnGetFormUnit(string PersonId, string PersonName)
        { // واحدهای یک شخص
            try
            {
                var UserId = Center.GetUserId(Request.Cookies["User.Aparteman.ir"]);

                Params = new Dictionary<string, object>
                {
                    { "@UserId", UserId },
                    { "@PersonId", PersonId },
                };
                ProcName = "UnitPersons_SearchUnit";

                ListUnit = await DBS.GetReportAsync(ProcName, Params);
                ViewData["PersonId"] = PersonId;
                ViewData["PersonName"] = PersonName;
                ViewData["Switch"] = "FormUnit";
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
        public async Task<IActionResult> OnGetFormSabtUnit(string ID, string PersonId, string PersonName)
        {// فرم تخصیص واحد به فرد
            try
            {
               
                ViewData["ID"] = ID;
                ViewData["PersonId"] = PersonId;
                ViewData["PersonName"] = PersonName;

                var UserId = Center.GetUserId(Request.Cookies["User.Aparteman.ir"]);

                // لیست ساختمانها
                Params = new Dictionary<string, object>
                {
                    { "@PersonId", UserId },
                };
                ProcName = "dbo.Buildings_ListPer";
                ListBuilding = await DBS.GetReportAsync(ProcName, Params);

                if (ListBuilding.Rows.Count == 1)
                {
                    // لیست طبقات
                    Params = new Dictionary<string, object>
                    {
                        { "@PersonId", UserId },
                        { "@BuildingId", ListBuilding.Rows[0]["BuildingId"] },
                    };
                    ProcName = "dbo.Floors_List";
                    ListFloor = await DBS.GetReportAsync(ProcName, Params);
                }

                if (ID != "0")
                {
                    // تخصیص
                    Params = new Dictionary<string, object>
                    {
                        { "@UserId", UserId },
                        { "@UnitPersonId", ID },
                    };
                    ProcName = "dbo.UnitPersons_Info";
                    RowData = await DBS.GetReportRowAsync(ProcName, Params);
                    if (RowData == null)
                    {
                        ViewData["Switch"] = "MSG";
                        ViewData["MSG"] = "اطلاعاتی پیدا نشد. دوباره تلاش کنید";

                        return new PartialViewResult
                        {
                            ViewName = "_CommonView",
                            ViewData = this.ViewData
                        };
                    };
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
        public async Task<IActionResult> OnGetUnitSituation(string UnitId)
        {// وضعیت مالکیت و سکونت فعلی واحد
            try
            {
                var userId = Center.GetUserId(Request.Cookies["User.Aparteman.ir"]);

                Params = new Dictionary<string, object>
                {
                    { "@UnitId" , UnitId } ,
                    { "@UserId" , userId } ,
                };
                ProcName = "dbo.Units_Info";
                RowData = await DBS.GetReportRowAsync(ProcName, Params);

                ViewData["Switch"] = "UnitSituation";
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
        public async Task<IActionResult> OnPostUpdateSabtUnit(string ID, string PersonId, string UnitId, string RoleCode, string DateStart)
        {//تخصیص واحد به شخص
            try
            {
                if (!await DBS.CheckToken(Center.TrimTxt(Request.Cookies["Aparteman.ir"]), userInfo))
                    return RedirectToPage("/LoginFast");

                Params = new Dictionary<string, object>
                {
                    { "@UnitPersonId", ID },
                    { "@UnitId", UnitId },
                    { "@PersonId", PersonId },
                    { "@RoleCode", RoleCode },
                    { "@StartDate", DateStart },
                    { "@EditorId", userInfo.Id },
                };

                ProcName = "dbo.UnitPersons_Save";
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




        public async Task<IActionResult> OnGetFormRelation(string PersonId, string PersonName)
        { // افراد مرتبط یک شخص
            try
            {
                var UserId = Center.GetUserId(Request.Cookies["User.Aparteman.ir"]);

                Params = new Dictionary<string, object>
                {
                    { "@UserId", UserId },
                    { "@PersonId", PersonId },
                };
                ProcName = "PersonRelation_Search";

                ListPersons = await DBS.GetReportAsync(ProcName, Params);
                ViewData["PersonId"] = PersonId;
                ViewData["PersonName"] = PersonName;
                ViewData["Switch"] = "FormRelation";
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
        public async Task<IActionResult> OnGetFormSabtRelation(string PersonId, string PersonName)
        {// فرم افزودن فرد مرتبط
            try
            {
                ViewData["PersonId"] = PersonId;
                ViewData["PersonName"] = PersonName;
                ViewData["Switch"] = "FormSabtRelation";
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
        public async Task<IActionResult> OnPostUpdateSabtRelation(string PersonId, string FullName, string RoleCode, string Phone, string Sharh)
        {//ثبت یک فرد مرتبط با شخص
            try
            {
                if (!await DBS.CheckToken(Center.TrimTxt(Request.Cookies["Aparteman.ir"]), userInfo))
                    return RedirectToPage("/LoginFast");

                Params = new Dictionary<string, object>
                {
                    { "@PersonId", PersonId },
                    { "@RoleCode", RoleCode },
                    { "@FullName", FullName },
                    { "@Phone", Phone },
                    { "@Sharh", Sharh },
                    { "@UserId", userInfo.Id },
                };

                ProcName = "dbo.PersonRelation_Save";
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
        public async Task<IActionResult> OnPostDeleteRelation(string PersonId, string RelationId)
        {//حذف یک فرد مرتبط با شخص
            try
            {
                if (!await DBS.CheckToken(Center.TrimTxt(Request.Cookies["Aparteman.ir"]), userInfo))
                    return RedirectToPage("/LoginFast");

                Params = new Dictionary<string, object>
                {
                    { "@PersonId", PersonId },
                    { "@RelationId", RelationId },
                    { "@UserId", userInfo.Id },
                };

                ProcName = "dbo.PersonRelation_Delete";
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




        public async Task<IActionResult> OnGetFormAsnad(string PersonId, string PersonName)
        { // لیست اسناد یک شخص
            try
            {
                var UserId = Center.GetUserId(Request.Cookies["User.Aparteman.ir"]);

                Params = new Dictionary<string, object>
                {
                    { "@UserId", UserId },
                    { "@PersonId", PersonId },
                };
                ProcName = "PersonAsnad_Search";

                ListAsnad = await DBS.GetReportAsync(ProcName, Params);
                ViewData["PersonId"] = PersonId;
                ViewData["PersonName"] = PersonName;
                ViewData["Switch"] = "FormAsnad";
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
        public async Task<IActionResult> OnGetFormSabtAsnad(string PersonId, string PersonName)
        {// فرم آپلود مدرک
            try
            {
                ViewData["PersonId"] = PersonId;
                ViewData["PersonName"] = PersonName;
                var UserId = Center.GetUserId(Request.Cookies["User.Aparteman.ir"]);

                Params = new Dictionary<string, object>
                {
                    { "@UserId", UserId },
                };
                ProcName = "Asnad_Active";

                ListAsnad = await DBS.GetReportAsync(ProcName, Params);

                ViewData["Switch"] = "FormSabtAsnad";
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




        public async Task<IActionResult> OnPostUpLoadFile(IFormFile FileUpload, string PersonId, string Sharh, string AsnadId)
        {// ✅ آپلود فایل (سند با نوع انتخابی)
            try
            {
                if (!await DBS.CheckToken(Center.TrimTxt(Request.Cookies["Aparteman.ir"]), userInfo))
                    return RedirectToPage("/LoginFast");

                // 🟡 اعتبارسنجی اولیه
                if (FileUpload == null || FileUpload.Length == 0)
                {
                    return Content("فایل را انتخاب کنید");
                }


                // 🟡 بررسی اکستنشن مجاز
                string ext = Path.GetExtension(FileUpload.FileName).ToUpperInvariant();
                var allowed = new[] { ".JPG", ".JPEG", ".PNG", ".BMP", ".PDF", ".TXT", ".XLS", ".XLSX", ".DOC", ".DOCX", ".ZIP", ".RAR" };
                if (!allowed.Contains(ext))
                {
                    return Content("فرمت فایل مجاز نیست.");
                }

                // 🟢 اجرای دستور SQL با 4 پارامتر (مطابق با SP)
                Params = new Dictionary<string, object>
                {
                    { "@PersonId", PersonId},
                    { "@UserId", userInfo.Id},
                    { "@AsnadId", AsnadId},
                    { "@Sharh", Sharh},
                    { "@Ext", ext},
                };

                ProcName = "dbo.PersonAsnad_Save";
                var SAVE = await DBS.GetReportRowAsync(ProcName, Params);
                string PersonAsnadId = "";
                if (SAVE["success"].ToString().ToUpper() == "OK")
                {
                    // آیدی ردیف ذخیره شده
                    PersonAsnadId = SAVE["PersonAsnadId"].ToString();
                }
                else
                {
                    return new JsonResult(SAVE);
                }
                // نام فایل براساس کد دریافتی از ذخیره مشخصات ساخته میشود
                Random random = new Random();
                int rnd = random.Next(10000, 99999);

                string fileName = $"S_{PersonAsnadId}_{rnd}{ext}";
                // محل ذخیره سند در پوشه Asnad است
                string folder = Path.Combine(_hostingEnvironment.WebRootPath, "Uploads", "Asnad");
                string filePath = Path.Combine(folder, fileName);
                // 🟡 حذف فایل قدیمی در صورت وجود
                Center.DeleteFileIfExists(filePath);

                // تصاویر تغییر سایز داده شوند
                var picFiles = new[] { ".JPG", ".JPEG", ".PNG", ".BMP" };
                if (picFiles.Contains(ext))
                {
                    // ذخیره موقت
                    string Temp = Path.Combine(folder, "temp");
                    Center.DeleteFileIfExists(Temp);
                    using (var stream = new FileStream(Temp, FileMode.Create))
                    {
                        FileUpload.CopyTo(stream);
                    }

                    // تغییر سایز تصویر
                    ImageResizer _Img = new ImageResizer(800, 800, ext);
                    _Img.Resize(Temp, filePath);
                }
                else
                {// سایر فایلها
                    // 🟢 ذخیره فایل روی دیسک
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        FileUpload.CopyTo(stream);
                    }
                }

                Params = new Dictionary<string, object>
                {
                    { "@PersonAsnadId", PersonAsnadId},
                    { "@NameFile", fileName},
                };

                ProcName = "dbo.PersonAsnad_SaveNameFile";
                var SaveNameFilew = await DBS.GetReportRowAsync(ProcName, Params);

                // ✅ موفقیت‌آمیز
                return new JsonResult(SaveNameFilew);
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
        public async Task<IActionResult> OnPostDeleteAsnad(string PersonId, string PersonAsnadId, string NameFile)
        { //حذف یک سند شخص
            try
            {
                if (!await DBS.CheckToken(Center.TrimTxt(Request.Cookies["Aparteman.ir"]), userInfo))
                    return RedirectToPage("/LoginFast");

                Params = new Dictionary<string, object>
                {
                    { "@PersonId", PersonId },
                    { "@PersonAsnadId", PersonAsnadId },
                    { "@UserId", userInfo.Id },
                };
                ProcName = "dbo.PersonAsnad_Delete";
                
                var Delete = await DBS.GetReportRowAsync(ProcName, Params);
                if (Delete["success"].ToString().ToUpper() == "OK")
                {
                    // محل ذخیره سند در پوشه Asnad است
                    string folder = Path.Combine(_hostingEnvironment.WebRootPath, "Uploads", "Asnad");
                    string filePath = Path.Combine(folder, NameFile);
                    // 🟡 حذف فایل قدیمی در صورت وجود
                    Center.DeleteFileIfExists(filePath);
                }

                return new JsonResult(Delete);

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




        public async Task<IActionResult> OnPostListKarbaran(string IsActive, string FullName
                    , string Mobile, string UnitNumber, string UserName, string Sort)
        {// لیست کاربران
            try
            {
                var UserId = Center.GetUserId(Request.Cookies["User.Aparteman.ir"]);

                Params = new Dictionary<string, object>
                {
                    { "@UserId", UserId },
                    { "@IsActive", IsActive },
                    { "@UnitNumber", UnitNumber },
                    { "@FullName", FullName },
                    { "@Mobile", Mobile },
                    { "@UserName", UserName},
                    { "@Sort", Sort },
                };
                ProcName = "Users_Search";


                ListPersons = await DBS.GetReportAsync(ProcName, Params);
                if (ListPersons.Rows.Count == 0)
                {
                    ViewData["Switch"] = "MSG";
                    ViewData["MSG"] = "کاربر با مشخصات فوق پیدا نشد. شرایط جستجو را تغییر بدهید و دوباره گزارش بگیرید";

                    return new PartialViewResult
                    {
                        ViewName = "_CommonView",
                        ViewData = this.ViewData
                    };
                };

                ViewData["Switch"] = "ListKarbaran";

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
        public async Task<IActionResult> OnGetFormSabtKarbaran(string PersonId)
        {// فرم ثبت دیتای کاربر
            try
            {
                ViewData["PersonId"] = PersonId;
                var UserId = Center.GetUserId(Request.Cookies["User.Aparteman.ir"]);

                Params = new Dictionary<string, object>
                {
                    { "@PersonId" , PersonId },
                    { "@UserId" , UserId },
                };
                ProcName = "Users_Info";
                RowData = await DBS.GetReportRowAsync(ProcName, Params);

                ViewData["Switch"] = "FormSabtKarbaran";
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
        public async Task<IActionResult> OnPostUpdateKarbar(string PersonId, string UserName, string IsActive)
        {//ثبت کاربر
            try
            {
                if (!await DBS.CheckToken(Center.TrimTxt(Request.Cookies["Aparteman.ir"]), userInfo))
                    return RedirectToPage("/LoginFast");

                Params = new Dictionary<string, object>
                {
                    { "@UserId", userInfo.Id } ,
                    { "@PersonId", PersonId },
                    { "@UserName", UserName },
                    { "@IsActive", IsActive },
                };
                ProcName = "Users_Save";
                var Save = await DBS.GetReportRowAsync(ProcName, Params);
                return new JsonResult(Save);

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
        public async Task<IActionResult> OnPostUpdatePass(string PersonId, string PASS)
        {//اصلاح پسور کاربر
            try
            {
                if (!await DBS.CheckToken(Center.TrimTxt(Request.Cookies["Aparteman.ir"]), userInfo))
                    return RedirectToPage("/LoginFast");
                PASS = Center.TrimTxt(PASS);
                if (PASS == "")
                {
                    return Content("رمز عبور جدید کاربر را بنویسید");
                }

                string PassNew = PasswordService.HashPassword(PASS);

                Params = new Dictionary<string, object>
                {
                    { "@UserId", userInfo.Id } ,
                    { "@PersonId", PersonId },
                    { "@NewPasswordHash", PassNew },
                };

                ProcName = "Users_ResetPasswordByAdmin";
                var resetPass = await DBS.GetReportRowAsync(ProcName, Params);

                return Content(resetPass["message"].ToString());

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
        public async Task<IActionResult> OnPostDeleteKarbar(string PersonId)
        {//حذف کاربر
            try
            {
                if (!await DBS.CheckToken(Center.TrimTxt(Request.Cookies["Aparteman.ir"]), userInfo))
                    return RedirectToPage("/LoginFast");

                if (PersonId == userInfo.Id.ToString())
                {
                    return Content("شما نمی توانید خودتان را حذف کنید");
                }
                Params = new Dictionary<string, object>
                {
                    { "@UserId", userInfo.Id } ,
                    { "@PersonId", PersonId } ,
                };

                ProcName = "Users_Delete";
                var _Delete = await DBS.GetReportResultAsync(ProcName, Params);
                return new JsonResult(_Delete);

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




        public async Task<IActionResult> OnGetFormSabtForms(string PersonId)
        {// مدیریت فرمهای کاربر
            try
            {
                ViewData["PersonId"] = PersonId;

                // تخصیص داده نشده لیست فرمها
                Params = new Dictionary<string, object>
                {
                    { "@Lg", "100" },
                    { "@PersonId", PersonId },
                };
                ProcName = "UserForms_Free";
                ListForms = await DBS.GetReportAsync(ProcName, Params);



                // فرمهای تخصیص داده شده
                Params = new Dictionary<string, object>
                {
                    { "@PersonId", PersonId },
                };
                ProcName = "UserForms_Active";
                ListData = await DBS.GetReportAsync(ProcName, Params);


                ViewData["Switch"] = "FormSabtForms";
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
        public async Task<IActionResult> OnPostUpdateAccessForm(string FFormId, string PersonId)
        {//تخصیص فرم به کاربر
            try
            {
                if (!await DBS.CheckToken(Center.TrimTxt(Request.Cookies["Aparteman.ir"]), userInfo))
                    return RedirectToPage("/LoginFast");

                Params = new Dictionary<string, object>
                {
                    { "@PersonId", PersonId } ,
                    { "@FormId", FFormId },
                    { "@UserId", userInfo.Id },
                };

                ProcName = "UserForms_Update";
                await DBS.RunCommandAsync(ProcName, Params);

                // تخصیص داده نشده لیست فرمها
                Params = new Dictionary<string, object>
                {
                    { "@Lg", "100" },
                    { "@PersonId", PersonId },
                };
                ProcName = "UserForms_Free";
                ListForms = await DBS.GetReportAsync(ProcName, Params);


                // فرمهای تخصیص داده شده
                Params = new Dictionary<string, object>
                {
                    { "@PersonId", PersonId },
                };
                ProcName = "UserForms_Active";
                ListData = await DBS.GetReportAsync(ProcName, Params);

                ViewData["Switch"] = "listForms";

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
