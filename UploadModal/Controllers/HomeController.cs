using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Helpers;
using System.IO;
using UploadModal.ViewModel;

namespace UploadModal.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Message = "Upload and crop images with preview";

            try
            {
                string[] filePaths = Directory.GetFiles(@Server.MapPath("~/Images/Cropped/"));
                var fullName = @"data:image/png;base64," + EncodeFile(filePaths[0]);

                HomeViewModel editor = new HomeViewModel
                {
                    UrlPhoto = fullName
                };

                return View(editor);
            }
            catch
            {
                return View(new HomeViewModel());
            }
        }

        public ActionResult Remove()
        {
            // cleaning directory just for demonstrations
            foreach (var f in Directory.GetFiles(@Server.MapPath("~/Images/Cropped/"))) System.IO.File.Delete(f);

            return View("Index", new HomeViewModel());
        }

        private string EncodeFile(string fileName)
        {
            return Convert.ToBase64String(System.IO.File.ReadAllBytes(fileName));
        }

        public ActionResult About()
        {
            return View();
        }
    }
}
