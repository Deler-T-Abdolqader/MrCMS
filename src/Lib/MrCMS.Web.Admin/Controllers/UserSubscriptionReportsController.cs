﻿using Microsoft.AspNetCore.Mvc;
using MrCMS.Web.Admin.ACL.UserSubscriptionReports;
using MrCMS.Web.Admin.Models.UserSubscriptionReports;
using MrCMS.Web.Admin.Services.UserSubscriptionReports;
using MrCMS.Website;
using MrCMS.Website.Controllers;

namespace MrCMS.Web.Admin.Controllers
{
    public class UserSubscriptionReportsController : MrCMSAdminController
    {
        private readonly IUserSubscriptionReportsService _userSubscriptionReportsService;

        public UserSubscriptionReportsController(IUserSubscriptionReportsService userSubscriptionReportsService)
        {
            _userSubscriptionReportsService = userSubscriptionReportsService;
        }

        [Acl(typeof(UserSubscriptionReportsACL), UserSubscriptionReportsACL.View)]
        public ViewResult Index(UserSubscriptionReportsSearchQuery searchQuery)
        {
            return View(searchQuery);
        }

        [Acl(typeof (UserSubscriptionReportsACL), UserSubscriptionReportsACL.View)]
        public JsonResult GraphData(UserSubscriptionReportsSearchQuery searchQuery)
        {
            var data = _userSubscriptionReportsService.GetAllSubscriptions(searchQuery);
            return Json(data);
        }
    }
}