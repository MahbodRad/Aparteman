using Aparteman.Models;
using Aparteman.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Data;
using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Aparteman.Pages.errors
{
    public class _500Model : PageModel
    {
        private string URL = "Home";
        private string VU = "Home_View";
        public Int32 FormId = 1;

        public UserInfo userInfo = new UserInfo();
        public FormData formData;

        //  [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]

        public string RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
        private Microsoft.AspNetCore.Hosting.IWebHostEnvironment _hostingEnvironment;
        public _500Model(Microsoft.AspNetCore.Hosting.IWebHostEnvironment hostingEnvironment)
        {// مسیر روت برنامه
            _hostingEnvironment = hostingEnvironment;
        }

        public void OnGet()
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        }

    }
}
