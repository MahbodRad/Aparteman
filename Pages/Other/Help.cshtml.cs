using Aparteman.Models;
using Aparteman.Services;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NPOI.OpenXmlFormats.Wordprocessing;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace Aparteman.Pages.Other
{
    public class HelpModel : PageModel
    {

        private string ProcName = ""; private Dictionary<string, object> Params;
        private string URL = "/Other/Help";
        private string VU = "Help_View";
        public Int32 FormId = 12;

        public UserInfo userInfo = new UserInfo();
        public FormData formData;

        public DataTable ListForms;
        public DataTable ListPages;
        public DataTable ListDevs;
        public DataTable ListDoucs;
        public DataTable ListClumns;
        public DataTable ListPics;
        public DataRow RowData;


        private Microsoft.AspNetCore.Hosting.IWebHostEnvironment _hostingEnvironment;
        public HelpModel(Microsoft.AspNetCore.Hosting.IWebHostEnvironment hostingEnvironment)
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
                    { "@Daste", "" },
                    { "@FRM", ""},
                    { "@Page", ""},
                };
                ProcName = "Forms_List";
                ListForms = await DBS.GetReportAsync(ProcName, Params);

                return Page();
            }
            catch (Exception ex)
            {
                await DBS.LogErrorSaveAsync(ProcName, Params, Center.GetUserId(Request.Cookies["User.Aparteman.ir"]), FormId, MethodBase.GetCurrentMethod().Name, ex.Message);
                return RedirectToPage("/Other/Khata", new { returnUrl = URL });
            }
        }

        public async Task<IActionResult> OnGetListForm(string FilterForm = "", string FilterPage = "", string Daste = "0")
        {
            try
            {
                Params = new Dictionary<string, object>
                {
                    { "@Daste", Daste },
                    { "@FRM", FilterForm},
                    { "@Page", FilterPage},
                };
                ProcName = "Forms_List";
                ListForms = await DBS.GetReportAsync(ProcName, Params);

                ViewData["Switch"] = "Forms";
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
        public async Task<IActionResult> OnGetFindMainHelp(string FormId)
        {
            try
            {
                Params = new Dictionary<string, object>
                {
                    { "@Si", FormId},
                };
                ProcName = "Forms_Show";
                RowData = await DBS.GetReportRowAsync(ProcName, Params);

                ViewData["Switch"] = "PageSub";
            }
            catch (Exception ex)
            {
                ViewData["ERRComm"] = await DBS.LogErrorSaveAsync(ProcName, Params, Center.GetUserId(Request.Cookies["User.Aparteman.ir"]), int.Parse(FormId), MethodBase.GetCurrentMethod().Name, ex.Message);
                ViewData["ERRRes"] = ex.Message;
                ViewData["Switch"] = "Error"; VU = "_CommonView";
            }
            return new PartialViewResult
            {
                ViewName = VU,
                ViewData = this.ViewData
            };
        }
        public async Task<IActionResult> OnGetFindPage(string FormId, string FormName)
        {

            try
            {
                ViewData["FormName"] = FormName;
                Params = new Dictionary<string, object>
                {
                    { "@Si_Form", FormId},
                };
                ProcName = "FormPages_List";
                ListPages = await DBS.GetReportAsync(ProcName, Params);

                ViewData["FormId"] = FormId;
                Params = new Dictionary<string, object>
                {
                    { "@Si", FormId},
                };
                ProcName = "Forms_Show";
                ViewData["FormAdres"] = await DBS.GetReportResultAsync(ProcName, Params, "NameForm");
                ViewData["Switch"] = "Pages";
            }
            catch (Exception ex)
            {
                ViewData["ERRComm"] = await DBS.LogErrorSaveAsync(ProcName, Params, Center.GetUserId(Request.Cookies["User.Aparteman.ir"]), int.Parse(FormId), MethodBase.GetCurrentMethod().Name, ex.Message);
                ViewData["ERRRes"] = ex.Message;
                ViewData["Switch"] = "Error"; VU = "_CommonView";
            }
            return new PartialViewResult
            {
                ViewName = VU,
                ViewData = this.ViewData
            };
        }
        public async Task<IActionResult> OnGetFindPageHelp(string ID = "0")
        {
            try
            {
                Params = new Dictionary<string, object>
                {
                    { "@Si", ID},
                };
                ProcName = "FormPages_Show";
                RowData = await DBS.GetReportRowAsync(ProcName, Params);

                ViewData["Switch"] = "PageDetail";
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

        public async Task<IActionResult> OnPostHelpSaveHeader(string FormId, string FormHeader)
        {
            try
            {
                if (! await DBS.CheckToken(Center.TrimTxt(Request.Cookies["Aparteman.ir"]), userInfo))
                    return RedirectToPage("/LoginFast");

                if (Center.TrimTxt(FormHeader).Length > 1000)
                {
                    return Content("متن راهنما خیلی طولانی میباشد. حداکثر 1000 کاراکتر مجاز میباشد");
                }
                Params = new Dictionary<string, object>
                {
                    { "@Si", FormId },
                    { "@Header", FormHeader },
                    { "@Editor", userInfo.Id },
                };
                ProcName = "Forms_UpdateHeader";
                await DBS.RunCommandAsync(ProcName, Params);
                
                return Content("OK");
            }
            catch (Exception ex)
            {
                ViewData["ERRComm"] = await DBS.LogErrorSaveAsync(ProcName, Params, Center.GetUserId(Request.Cookies["User.Aparteman.ir"]), int.Parse(FormId), MethodBase.GetCurrentMethod().Name, ex.Message);
                ViewData["ERRRes"] = ex.Message;
                ViewData["Switch"] = "Error"; VU = "_CommonView";
            }
            return new PartialViewResult
            {
                ViewName = VU,
                ViewData = this.ViewData
            };
        }
        public async Task<IActionResult> OnPostHelpSaveTp(string FormId, string FormTp)
        {
            try
            {
                if (! await DBS.CheckToken(Center.TrimTxt(Request.Cookies["Aparteman.ir"]), userInfo))
                    return RedirectToPage("/LoginFast");

                Params = new Dictionary<string, object>
                {
                    { "@Si", FormId },
                    { "@Help", FormTp },
                    { "@Editor", userInfo.Id },
                };
                ProcName = "Forms_UpdateTp";
                await DBS.RunCommandAsync(ProcName, Params);
                

                return Content("OK");
            }
            catch (Exception ex)
            {
                ViewData["ERRComm"] = await DBS.LogErrorSaveAsync(ProcName, Params, Center.GetUserId(Request.Cookies["User.Aparteman.ir"]), int.Parse(FormId), MethodBase.GetCurrentMethod().Name, ex.Message);
                ViewData["ERRRes"] = ex.Message;
                ViewData["Switch"] = "Error"; VU = "_CommonView";
            }
            return new PartialViewResult
            {
                ViewName = VU,
                ViewData = this.ViewData
            };
        }
        public async Task<IActionResult> OnPostHelpSaveFooter(string FormId, string FormFooter)
        {
            try
            {
                if (! await DBS.CheckToken(Center.TrimTxt(Request.Cookies["Aparteman.ir"]), userInfo))
                    return RedirectToPage("/LoginFast");

                if (Center.TrimTxt(FormFooter).Length > 1000)
                {
                    return Content("متن راهنما خیلی طولانی میباشد. حداکثر 1000 کاراکتر مجاز میباشد");
                }

                Params = new Dictionary<string, object>
                {
                    { "@Si", FormId },
                    { "@Footer", FormFooter },
                    { "@Editor", userInfo.Id },
                };
                ProcName = "Forms_UpdateFooter";
                await DBS.RunCommandAsync(ProcName, Params);
                

                return Content("OK");
            }
            catch (Exception ex)
            {
                ViewData["ERRComm"] = await DBS.LogErrorSaveAsync(ProcName, Params, Center.GetUserId(Request.Cookies["User.Aparteman.ir"]), int.Parse(FormId), MethodBase.GetCurrentMethod().Name, ex.Message);
                ViewData["ERRRes"] = ex.Message;
                ViewData["Switch"] = "Error"; VU = "_CommonView";
            }
            return new PartialViewResult
            {
                ViewName = VU,
                ViewData = this.ViewData
            };
        }

        public async Task<IActionResult> OnGetListPics(string PageId = "0")
        {
            try
            {
                Params = new Dictionary<string, object>
                {
                    { "@Si_Page", PageId},
                };
                ProcName = "FormPics_List";
                ListPics = await DBS.GetReportAsync(ProcName, Params);

                //Uri Urladdress = new Uri(Request.Host.ToString());
                ViewData["FilePath"] = "https://Aparteman.ir/Uploads/Help/Pic";
                ViewData["Page"] = PageId;
                ViewData["Switch"] = "Pictures";
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

        public async Task<IActionResult> OnPostSavePictur(IFormFile PhotoUpload, string PageId = "0", string Tp = "")
        {
            try
            {
                if (! await DBS.CheckToken(Center.TrimTxt(Request.Cookies["Aparteman.ir"]), userInfo))
                    return RedirectToPage("/LoginFast");

                if (PhotoUpload != null)
                {
                    string Temp = @"Upload\Help\Temp";
                    string folderName = @"Upload\Help";
                    string webRootPath = _hostingEnvironment.WebRootPath;
                    Temp = Path.Combine(webRootPath, Temp);
                    folderName = Path.Combine(webRootPath, folderName);

                    string ext = Path.GetExtension(PhotoUpload.FileName);
                    ext = ext.ToUpper();
                    if (ext == ".JPG" || ext == ".JPEG" || ext == ".PNG" || ext == ".BMP")
                    {

                        Params = new Dictionary<string, object>
                        {
                            { "@Si_Page", PageId },
                            { "@Ext", ext },
                            { "@Tp", Tp},
                        };  
                        ProcName = "FormPics_Add";
                        var SI = await DBS.GetReportResultAsync(ProcName, Params,"Si");
                        

                        string fullPath = Path.Combine(folderName, "Pic_" + SI + ext);
                        Center.DeleteFileIfExists(fullPath);
                        Center.DeleteFileIfExists(Temp);

                        using (var stream = new FileStream(Temp, FileMode.Create))
                        {
                            PhotoUpload.CopyTo(stream);
                        }
                        ImageResizer _Img = new ImageResizer(500, 500, ext);
                        _Img.Resize(Temp, fullPath);

                    }
                    else
                    {
                        ViewData["Switch"] = "MSG";
                        ViewData["MSG"] = "تصویر ذخیره نشد. لطفا از تصاویر با فرمت JPG استفاده کنید";
                        return new PartialViewResult
                        {
                            ViewName = "_CommonView",
                            ViewData = this.ViewData
                        };
                    }
                }
                else
                {
                    ViewData["Switch"] = "MSG";
                    ViewData["MSG"] = "فایل تصویری را انتخاب کنید";
                    return new PartialViewResult
                    {
                        ViewName = "_CommonView",
                        ViewData = this.ViewData
                    };
                }

                Params = new Dictionary<string, object>
                {
                    { "@Si_Page", PageId},
                };
                ProcName = "FormPics_List";
                ListPics = await DBS.GetReportAsync(ProcName, Params);

                ViewData["Page"] = PageId;
                ViewData["Switch"] = "ListPictures";
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
        public async Task<IActionResult> OnPostPhotoDelete(string Id = "0", string PhotoExt = ".jpg")
        {
            try
            {
                if (! await DBS.CheckToken(Center.TrimTxt(Request.Cookies["Aparteman.ir"]), userInfo))
                    return RedirectToPage("/LoginFast");
                // حذف از دیتابیس
                Params = new Dictionary<string, object>
                {
                    { "@Si", Id},
                };
                ProcName = "FormPics_Delete";
                await DBS.RunCommandAsync(ProcName, Params);

                // حذف فیزیکی فایل تصویر
                string folderName = @"Upload\Help\Pic_" + Id + PhotoExt;
                string webRootPath = _hostingEnvironment.WebRootPath;
                string PicPath = Path.Combine(webRootPath, folderName);
                Center.DeleteFileIfExists(PicPath);
                return Content("OK");
            }
            catch (Exception ex)
            {
                ViewData["ERRComm"] = await DBS.LogErrorSaveAsync(ProcName, Params, Center.GetUserId(Request.Cookies["User.Aparteman.ir"]), FormId, MethodBase.GetCurrentMethod().Name, ex.Message);
                ViewData["ERRRes"] = ex.Message;
                ViewData["Switch"] = "Error"; VU = "_CommonView";
                return new PartialViewResult
                {
                    ViewName = VU,
                    ViewData = this.ViewData
                };
            }
        }
        public async Task<IActionResult> OnPostHelpNameSave(string FormId, string Reg, string Onvan, string Tp)
        {
            try
            {
                if (! await DBS.CheckToken(Center.TrimTxt(Request.Cookies["Aparteman.ir"]), userInfo))
                    return RedirectToPage("/LoginFast");

                Params = new Dictionary<string, object>
                {
                    { "@Si", FormId },
                    { "@Reg", Reg },
                    { "@Onvan", Onvan},
                    { "@Tp", Tp},
                    { "@Editor", userInfo.Id},
                };
                ProcName = "Forms_UpdateName";
                await DBS.RunCommandAsync(ProcName, Params);
                return Content("OK");
            }
            catch (Exception ex)
            {
                ViewData["ERRComm"] = await DBS.LogErrorSaveAsync(ProcName, Params, Center.GetUserId(Request.Cookies["User.Aparteman.ir"]), int.Parse(FormId), MethodBase.GetCurrentMethod().Name, ex.Message);
                ViewData["ERRRes"] = ex.Message;
                ViewData["Switch"] = "Error"; VU = "_CommonView";
                return new PartialViewResult
                {
                    ViewName = VU,
                    ViewData = this.ViewData
                };
            }
        }

        public async Task<IActionResult> OnPostPageSaveHeader(string PageId = "0", string HeaderTp = "")
        {
            try
            {
                if (! await DBS.CheckToken(Center.TrimTxt(Request.Cookies["Aparteman.ir"]), userInfo))
                    return RedirectToPage("/LoginFast");
                if (Center.TrimTxt(HeaderTp).Length > 2000)
                {
                    return Content("متن راهنمای هدر خیلی طولانی میباشد. حداکثر 2000 کاراکتر مجاز میباشد");
                }

                Params = new Dictionary<string, object>
                {
                    { "@Si", PageId },
                    { "@Header", HeaderTp},
                    { "@Editor", userInfo.Id},
                };  

                ProcName = "FormPages_UpdateHeader";
                await DBS.RunCommandAsync(ProcName, Params);
                

                return Content("OK");
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
        public async Task<IActionResult> OnPostPageSaveTp(string PageId = "0", string FormTp = "")
        {
            try
            {
                if (! await DBS.CheckToken(Center.TrimTxt(Request.Cookies["Aparteman.ir"]), userInfo))
                    return RedirectToPage("/LoginFast");

                Params = new Dictionary<string, object>
                {
                    { "@Si", PageId },
                    { "@Help", FormTp},
                    { "@Editor", userInfo.Id},
                };
                ProcName = "FormPages_UpdateTp";
                await DBS.RunCommandAsync(ProcName, Params);
                
                return Content("OK");
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
        public async Task<IActionResult> OnPostUpdateOnvanRj(string PageId = "0", string PageOnvan = "0", string PageRj = "0")
        {
            try
            {
                if (! await DBS.CheckToken(Center.TrimTxt(Request.Cookies["Aparteman.ir"]), userInfo))
                    return RedirectToPage("/LoginFast");

                Params = new Dictionary<string, object>
                {
                    { "@Si", PageId },
                    { "@RJ", PageRj },
                    { "@Onvan", PageOnvan},
                    { "@Editor", userInfo.Id},
                };
                ProcName = "FormPages_UpdateOnvanRj";
                await DBS.RunCommandAsync(ProcName, Params);

                return Content("OK");
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
        public async Task<IActionResult> OnPostPageDelete(string PageId = "0")
        {
            try
            {
                if (! await DBS.CheckToken(Center.TrimTxt(Request.Cookies["Aparteman.ir"]), userInfo))
                    return RedirectToPage("/LoginFast");

                Params = new Dictionary<string, object>
                {
                    { "@Si", PageId},
                };
                ProcName = "FormPages_Delete";
                await DBS.RunCommandAsync(ProcName, Params);
                // حذف تصاویر انجام شد
                return Content("OK");
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
        public async Task<IActionResult> OnPostAddNewPage(string FormId, string PageName)
        {
            try
            {
                if (! await DBS.CheckToken(Center.TrimTxt(Request.Cookies["Aparteman.ir"]), userInfo))
                    return RedirectToPage("/LoginFast");

                Params = new Dictionary<string, object>
                {
                    { "@Si_Form", FormId },
                    { "@Name", PageName},
                    { "@Editor", userInfo.Id},
                };

                ProcName = "FormPages_Add";
                await DBS.RunCommandAsync(ProcName, Params);

                return Content("OK");
            }
            catch (Exception ex)
            {
                ViewData["ERRComm"] = await DBS.LogErrorSaveAsync(ProcName, Params, Center.GetUserId(Request.Cookies["User.Aparteman.ir"]), int.Parse(FormId), MethodBase.GetCurrentMethod().Name, ex.Message);
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
