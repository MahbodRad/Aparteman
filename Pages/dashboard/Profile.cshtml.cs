using Aparteman.Models;
using Aparteman.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;
using System.Reflection;

namespace Aparteman.Pages.dashboard
{
    public class ProfileModel : PageModel
    {
        private string ProcName = ""; private Dictionary<string, object> Params;
        private string URL = "Profile";
        private string VU = "Profile_View";
        public Int32 FormId = 16;

        public UserInfo userInfo = new UserInfo();
        public required FormData formData;

        public required DataRow UserData;
        public required DataTable ListLogin;
        public required DataTable ListAsnad;
        //public DataTable ListData;
        private Microsoft.AspNetCore.Hosting.IWebHostEnvironment _hostingEnvironment;
        public ProfileModel(Microsoft.AspNetCore.Hosting.IWebHostEnvironment hostingEnvironment)
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



                Params = new Dictionary<string, object>
                {
                    { "@PersonId", userInfo.Id},
                };
                ProcName = "Users_InfoPer";
                UserData = await DBS.GetReportRowAsync(ProcName, Params);

                Params = new Dictionary<string, object>
                {
                    { "@PersonId", userInfo.Id},
                };
                ProcName = "LastVisit_IP";
                ListLogin = await DBS.GetReportAsync(ProcName, Params);


                return Page();
            }
            catch (Exception ex)
            {
                ViewData["ERRComm"] = await DBS.LogErrorSaveAsync(ProcName, Params, Center.GetUserId(Request.Cookies["User.Aparteman.ir"]), FormId, MethodBase.GetCurrentMethod().Name, ex.Message);
                return RedirectToPage("/Other/Khata", new { returnUrl = URL });
            }
        }

        // ✅ به‌روزرسانی دیتای کاربر
        public async Task<IActionResult> OnPostUpdateUserInfo(string FName, string LName, string Responsible, string Phone, string NationalCode, string Mobile, string Email, string Post, string Adres)
        {
            try
            {
                if (! await DBS.CheckToken(Center.TrimTxt(Request.Cookies["Aparteman.ir"]), userInfo))
                    return RedirectToPage("/LoginFast");

                Params = new Dictionary<string, object>
                {
                    { "@Id_User", userInfo.Id } ,
                    { "@FName" , FName } ,
                    { "@LName" , LName } ,
                    { "@Phone" , Phone } ,
                    { "@CodeMeli" , NationalCode } ,
                    { "@Mobile" , Mobile } ,
                    { "@Email" , Email } ,
                    { "@Post" , Post } ,
                    { "@Adres" , Adres } ,
                    { "@Responsible" , Responsible } ,
                };

                ProcName = "Persons_UpdateInfo";
                return Content(await DBS.GetReportResultAsync(ProcName, Params, "Res"));
            }
            catch (Exception ex)
            {
                ViewData["ERRComm"] = await DBS.LogErrorSaveAsync(ProcName, Params, Center.GetUserId(Request.Cookies["User.Aparteman.ir"]), FormId, MethodBase.GetCurrentMethod().Name, ex.Message);
                ViewData["ERRRes"] = ex.Message;
                ViewData["Switch"] = "Error";
                return new PartialViewResult { ViewName = "_CommonView", ViewData = ViewData };
            }
        }

        // ✅ تغییر رمز عبور
        public async Task<IActionResult> OnPostUpdatePass(string UserName, string PassNow, string PassNew)
        {
            try
            {
                if (! await DBS.CheckToken(Center.TrimTxt(Request.Cookies["Aparteman.ir"]), userInfo))
                    return RedirectToPage("/LoginFast");

                UserName = Center.TrimTxt(UserName);
                PassNow = Center.TrimTxt(PassNow);
                PassNew = Center.TrimTxt(PassNew);

                string Res = "توجه: ";
                if (UserName == "") Res += "\n نام کاربری خود را بنویسید";
                if (PassNow == "") Res += "\n رمز فعلی را بنویسید";
                if (PassNew == "") Res += "\n رمز جدید را بنویسید";

                if (Res == "توجه: ")
                {
                    PassNow = PasswordService.HashPassword(PassNow);
                    PassNew = PasswordService.HashPassword(PassNew);

                    Params = new Dictionary<string, object>
                    {
                        { "@PersonId", userInfo.Id },
                        { "@UserName", UserName },
                        { "@PasswordHashNow", PassNow },
                        { "@PasswordHashNew", PassNew},
                    };
                    ProcName = "Users_UpdatePas";
                    var ResRow = await DBS.GetReportRowAsync(ProcName, Params);

                    Res = ResRow["message"].ToString();

                }

                return Content(Res);
            }
            catch (Exception ex)
            {
                ViewData["ERRComm"] = await DBS.LogErrorSaveAsync(ProcName, Params, Center.GetUserId(Request.Cookies["User.Aparteman.ir"]), FormId, MethodBase.GetCurrentMethod().Name, ex.Message);
                ViewData["ERRRes"] = ex.Message;
                ViewData["Switch"] = "Error";

                return new PartialViewResult { ViewName = "_CommonView", ViewData = ViewData };
            }
        }

        // ✅ آپلود تصویر پروفایل
        public async Task<IActionResult> OnPostUpLoadPic(IFormFile PicUpload, string OldPic = "")
        {
            try
            {
                if (! await DBS.CheckToken(Center.TrimTxt(Request.Cookies["Aparteman.ir"]), userInfo))
                    return RedirectToPage("/LoginFast");

                string folder = Path.Combine(_hostingEnvironment.WebRootPath, "Uploads", "Persons");

                string ext = Path.GetExtension(PicUpload.FileName).ToUpperInvariant();
                Random random = new Random();
                int Scr = random.Next(1000, 9999);

                if (ext is ".JPG" or ".JPEG" or ".PNG" or ".BMP")
                {
                    string fileName = Center.TrimTxt(OldPic);
                    string path = Path.Combine(folder, fileName);
                    if (fileName != "K_0.jpg")
                    {
                        Center.DeleteFileIfExists(path);
                    }

                    fileName = $"K_{userInfo.Id}_{Scr}{ext}";
                    path = Path.Combine(folder, fileName);

                    // ذخیره موقت
                    string Temp = Path.Combine(folder, "temp");
                    Center.DeleteFileIfExists(Temp);
                    using (var stream = new FileStream(Temp, FileMode.Create))
                    {
                        PicUpload.CopyTo(stream);
                    }

                    // تغییر سایز تصویر
                    ImageResizer _Img = new ImageResizer(300, 300, ext);
                    _Img.Resize(Temp, path);

                    // اصلاح نام تصویر برای کاربر
                    string ScrExt = $"{Scr}{ext}";
                    Params = new Dictionary<string, object>
                    {
                        { "@Si_Person", userInfo.Id },
                        { "@Pic", ScrExt},
                    };
                    ProcName = "Persons_UpdatePic";
                    await DBS.RunCommandAsync(ProcName, Params);

                    return Content("تصویر با موفقیت ذخیره شد");
                }

                return Content("فقط فرمت‌های تصویری مجازند");
            }
            catch (Exception ex)
            {
                ViewData["ERRComm"] = await DBS.LogErrorSaveAsync(ProcName, Params, Center.GetUserId(Request.Cookies["User.Aparteman.ir"]), FormId, MethodBase.GetCurrentMethod().Name, ex.Message);
                ViewData["ERRRes"] = ex.Message;
                ViewData["Switch"] = "Error";

                return new PartialViewResult { ViewName = "_CommonView", ViewData = ViewData };
            }
        }


        public async Task<IActionResult> OnGetFormSabtAsnad()
        {// فرم آپلود اسناد
            try
            {
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


        public async Task<IActionResult> OnPostUpLoadFile(IFormFile FileUpload, string Sharh, string AsnadId)
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
                    { "@PersonId", userInfo.Id},
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
                ViewData["ERRComm"] = await DBS.LogErrorSaveAsync(ProcName, Params, Center.GetUserId(Request.Cookies["User.BenIce.ir"]), FormId, MethodBase.GetCurrentMethod().Name, ex.Message);
                ViewData["ERRRes"] = ex.Message;
                ViewData["Switch"] = "Error"; VU = "_CommonView";

            }
            return new PartialViewResult
            {
                ViewName = VU,
                ViewData = this.ViewData
            };

        }
        public async Task<IActionResult> OnGetListAsnad()
        { // لیست اسناد یک شخص
            try
            {
                var UserId = Center.GetUserId(Request.Cookies["User.Aparteman.ir"]);

                Params = new Dictionary<string, object>
                {
                    { "@UserId", UserId },
                    { "@PersonId", UserId },
                };
                ProcName = "PersonAsnad_Search";
                ListAsnad = await DBS.GetReportAsync(ProcName, Params);
                if (ListAsnad.Rows.Count == 0)
                {
                    ViewData["Switch"] = "MSG";
                    ViewData["MSG"] = "هنوز فایل یا سندی را آپلود نکرده اید.";

                    return new PartialViewResult
                    {
                        ViewName = "_CommonView",
                        ViewData = this.ViewData
                    };
                }
                ViewData["Switch"] = "ListAsnad";
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
