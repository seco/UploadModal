using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using UploadModal.ViewModel;
using System.Web.Helpers;

namespace UploadModal.Controllers
{
    public class UploadController : Controller
    {
        private string StorageRoot
        {
            get { return Path.Combine(Server.MapPath("~/Images/Original/")); }
        }

        private string StorageCrop
        {
            get { return Path.Combine(Server.MapPath("~/Images/Cropped/")); }
        }

        public ActionResult Index()
        {
            return PartialView();
        }

        public ActionResult Edit(EditorInputViewModel editor)
        {
            var image = new WebImage(StorageRoot + editor.Imagem);

            int _height = (int)editor.Height;
            int _width = (int)editor.Width;

            if (_width > 0 && _height > 0)
            {
                image.Resize(_width, _height, false, false);

                int _top = (int)(editor.Top);
                int _bottom = (int)editor.Bottom;
                int _left = (int)editor.Left;
                int _right = (int)editor.Right;

                _height = (int)image.Height;
                _width = (int)image.Width;

                image.Crop(_top, _left, (_height - _bottom), (_width - _right));
            }

            var originalFile = editor.Imagem;
            editor.Imagem = Url.Content(StorageCrop + Path.GetFileName(image.FileName));
            image.Resize(150, 150, true, false);
            image.Save(editor.Imagem);

            //System.IO.File.Delete(Server.MapPath(originalFile));

            return View("Index", editor);
        }

        [HttpGet]
        public void Delete(string id)
        {
            var filename = id;
            var filePath = Path.Combine(StorageRoot, filename);

            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
        }

        [HttpGet]
        public void Download(string id)
        {
            var filename = id;
            var filePath = Path.Combine(StorageRoot, filename);

            var context = HttpContext;

            if (System.IO.File.Exists(filePath))
            {
                context.Response.AddHeader("Content-Disposition", "attachment; filename=\"" + filename + "\"");
                context.Response.ContentType = "application/octet-stream";
                context.Response.ClearContent();
                context.Response.WriteFile(filePath);
            }
            else
                context.Response.StatusCode = 404;
        }

        [HttpPost]
        public ActionResult UploadFiles()
        {
            var r = new List<UploadFilesResultViewModel>();

            foreach (string file in Request.Files)
            {
                var statuses = new List<UploadFilesResultViewModel>();
                var headers = Request.Headers;

                if (string.IsNullOrEmpty(headers["X-File-Name"]))
                {
                    UploadWholeFile(Request, statuses);
                }
                else
                {
                    UploadPartialFile(headers["X-File-Name"], Request, statuses);
                }

                JsonResult result = Json(statuses);
                result.ContentType = "text/plain";

                return result;
            }

            return Json(r);
        }

        private string EncodeFile(string fileName)
        {
            return Convert.ToBase64String(System.IO.File.ReadAllBytes(fileName));
        }

        //Credit to i-e-b and his ASP.Net uploader for the bulk of the upload helper methods - https://github.com/i-e-b/jQueryFileUpload.Net
        private void UploadPartialFile(string fileName, HttpRequestBase request, List<UploadFilesResultViewModel> statuses)
        {
            if (request.Files.Count != 1) throw new HttpRequestValidationException("Attempt to upload chunked file containing more than one fragment per request");
            var file = request.Files[0];
            var inputStream = file.InputStream;

            var fullName = Path.Combine(StorageRoot, Path.GetFileName(fileName));

            using (var fs = new FileStream(fullName, FileMode.Append, FileAccess.Write))
            {
                var buffer = new byte[1024];

                var l = inputStream.Read(buffer, 0, 1024);
                while (l > 0)
                {
                    fs.Write(buffer, 0, l);
                    l = inputStream.Read(buffer, 0, 1024);
                }
                fs.Flush();
                fs.Close();
            }

            var image = new WebImage(StorageRoot + fileName);

            statuses.Add(new UploadFilesResultViewModel()
            {
                name = fileName,
                size = file.ContentLength,
                type = file.ContentType,
                url = "/Upload/Download/" + fileName,
                delete_url = "/Upload/Delete/" + fileName,
                thumbnail_url = @"data:image/png;base64," + EncodeFile(fullName),
                delete_type = "GET",


                Width = image.Width,
                Height = image.Height,
                Top = image.Height * 0.1,
                Left = image.Width * 0.9,
                Right = image.Width * 0.9,
                Bottom = image.Height * 0.9
            });
        }

        //Credit to i-e-b and his ASP.Net uploader for the bulk of the upload helper methods - https://github.com/i-e-b/jQueryFileUpload.Net
        private void UploadWholeFile(HttpRequestBase request, List<UploadFilesResultViewModel> statuses)
        {
            for (int i = 0; i < request.Files.Count; i++)
            {
                var file = request.Files[i];

                var fullPath = Path.Combine(StorageRoot, Path.GetFileName(file.FileName));

                file.SaveAs(fullPath);

                var image = new WebImage(StorageRoot + file.FileName);

                statuses.Add(new UploadFilesResultViewModel()
                {
                    name = file.FileName,
                    size = file.ContentLength,
                    type = file.ContentType,
                    url = "/Upload/Download/" + file.FileName,
                    delete_url = "/Upload/Delete/" + file.FileName,
                    thumbnail_url = @"data:image/png;base64," + EncodeFile(fullPath),
                    delete_type = "GET",

                    Width = image.Width,
                    Height = image.Height,
                    Top = image.Height * 0.1,
                    Left = image.Width * 0.9,
                    Right = image.Width * 0.9,
                    Bottom = image.Height * 0.9
                });
            }
        }
    }
}
