using Aparteman.Models;
using Aparteman.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using System;

namespace Aparteman.Pages.Shared.Components
{
    public class LayoutInfoViewComponent : ViewComponent
    {
        private FormSetup FormSet = new FormSetup();
        private List<ListForm> ListForms = new List<ListForm>();


        public async Task<IViewComponentResult> InvokeAsync(Int32 FormId, UserInfo userInfo)
        {
            var data = await GetLayoutDataAsync(FormId, userInfo); // تابعی برای دریافت اطلاعات پویا
            return View("HeaderData", data);
        }
        private async Task<LayoutDataModel> GetLayoutDataAsync(Int32 FormId, UserInfo userInfo)
        {

            await FormInfo(FormId, userInfo);

            ViewData["Script"] = FormSet.Script;
            ViewData["Titel"] = FormSet.Title;
          
            var model = new LayoutDataModel
            { 
                userInfo = userInfo,
                formSetup = FormSet,
                listForms = ListForms,
            };

            HttpContext.Items["LayoutData"] = model;

            return model;
        }

        private async Task FormInfo(Int32 FormId, UserInfo userInfo)
        { 
          // لیست فرمهایی که به کاربر تخصیص داده شده است
          // دیتای ثابت روی فرم
            FormSet.FormId = FormId;
            FormSet.Script = $"~/js/page_{FormId}.js";
            FormSet.UserId = userInfo.Id;
            FormSet.UserName = userInfo.Name;

            // لیست دسته بندی منوهایی که به کاربر تخصیص داده شده است؟
            var parametrs = new Dictionary<string, object>
            {
                { "@PersonId", userInfo.Id }
            };
            ListForm _Frm;
            // لیست فرمهایی که به کاربر تخصیص داده شده است
            var listFormTBL = await DBS.GetReportAsync("dbo.UserForms_Active", parametrs);
            foreach (DataRow item in listFormTBL.Rows)
            {
                _Frm = new ListForm()
                {
                    Title = item["FormTitle"].ToString(),
                    Adres = item["Route"].ToString(),
                    
                    Css = "menu-ItemPre",
                    
                };

                if (item.Field<Int32>("FormId") == FormId)
                {
                    _Frm.Css = "menu-form-Item";
                    FormSet.Title = item["FormTitle"].ToString();
                    FormSet.Header = item["Header"].ToString();
                    FormSet.Footer = item["Footer"].ToString();
                }

                ListForms.Add(_Frm);
            }

            FormSet.Today = await DBS.Emruzasync();
        }
    }
}
