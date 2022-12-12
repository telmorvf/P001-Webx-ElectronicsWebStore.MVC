using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;
using Webx.Web.Helpers;
using Webx.Web.Models;

namespace Webx.Web.Controllers
{
    public class ContactController : Controller
    {
        private readonly IMailHelper _mailHelper;
        private readonly INotyfService _toastNotification;

        public ContactController(IMailHelper mailHelper, INotyfService toastNotification)
        {
            _mailHelper = mailHelper;
            _toastNotification = toastNotification;
        }

        public IActionResult Announcement()
        {
            var model = new AnnouncementViewModel
            {
                To = _mailHelper.Destinations()
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Announcement(AnnouncementViewModel model)
        {
            if (ModelState.IsValid)
            {
                var path = "";

                if (model.Attachment != null)
                {
                    path = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\tempData\" + model.Attachment.FileName);

                    using (var stream = System.IO.File.Create(path))
                    {
                        await model.Attachment.CopyToAsync(stream);
                    }
                }

                var response = await _mailHelper.SendAnnouncementAsync(model.ToId, model.Title, model.Message, path);

                if (!string.IsNullOrEmpty(path))
                {
                    System.IO.File.Delete(path);
                }

                if (response.IsSuccess == true)
                {
                    _toastNotification.Success("The annoucment was successfuly sent!");
                    return RedirectToAction(nameof(Announcement));
                }
                else
                {
                    _toastNotification.Warning("There was an error sending the announcement!");
                    model.To = _mailHelper.Destinations();
                    return View(model);
                }
            }

            model.To = _mailHelper.Destinations();
            _toastNotification.Warning("There was an error sending the announcement!");
            return View(model);
        }

        [HttpPost]
        [Route("Contact/ToastNotification")]
        public JsonResult ToastNotification(string message, string type)
        {
            bool result = false;

            if (type == "success")
            {
                _toastNotification.Success(message, 5);
                result = true;
            }

            if (type == "error")
            {
                _toastNotification.Error(message, 5);
                result = true;
            }

            if (type == "warning")
            {
                _toastNotification.Warning(message, 5);
                result = true;
            }

            if (type == "Information")
            {
                _toastNotification.Information(message, 5);
                result = true;
            }

            return Json(result);
        }
    }
}
