﻿using Microsoft.AspNetCore.Mvc;
using MrCMS.Entities.Notifications;
using MrCMS.Web.Admin.ACL;
using MrCMS.Web.Admin.Models;
using MrCMS.Web.Admin.Services;
using MrCMS.Website;
using MrCMS.Website.Controllers;

namespace MrCMS.Web.Admin.Controllers
{
    public class NotificationController : MrCMSAdminController
    {
        private readonly INotificationAdminService _service;

        public NotificationController(INotificationAdminService service)
        {
            _service = service;
        }

        public ViewResult Index(NotificationSearchQuery searchQuery)
        {
            ViewData["results"] = _service.Search(searchQuery);
            ViewData["notification-type-options"] = _service.GetNotificationTypeOptions(true);
            return View(searchQuery);
        }

        [HttpGet]
        public ViewResult Push()
        {
            ViewData["publish-type-options"] = _service.GetPublishTypeOptions();
            ViewData["notification-type-options"] = _service.GetNotificationTypeOptions();
            return View(new NotificationModel());
        }

        [HttpPost]
        public RedirectToActionResult Push(NotificationModel model)
        {
            _service.PushNotification(model);
            return RedirectToAction("Index");
        }

        [HttpGet]
        [Acl(typeof(NotificationACL), NotificationACL.Delete)]
        public ViewResult Delete(Notification notification)
        {
            return View(notification);
        }

        [HttpPost]
        [ActionName("Delete")]
        [Acl(typeof(NotificationACL), NotificationACL.Delete)]
        public RedirectToActionResult Delete_POST(Notification notification)
        {
            _service.Delete(notification);
            return RedirectToAction("Index");
        }
    }
}