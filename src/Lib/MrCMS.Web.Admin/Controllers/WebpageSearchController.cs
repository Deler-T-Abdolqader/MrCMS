﻿using Microsoft.AspNetCore.Mvc;
using MrCMS.Web.Admin.Models;
using MrCMS.Web.Admin.Services;
using MrCMS.Website.Controllers;

namespace MrCMS.Web.Admin.Controllers
{
    public class WebpageSearchController : MrCMSAdminController
    {
        private readonly IWebpageAdminSearchService _webpageAdminSearchService;

        public WebpageSearchController(IWebpageAdminSearchService webpageAdminSearchService)
        {
            _webpageAdminSearchService = webpageAdminSearchService;
        }

        public ViewResult Search(WebpageSearchQuery searchQuery)
        {
            ViewData["results"] = _webpageAdminSearchService.Search(searchQuery);
            return View(searchQuery);
        }

        public PartialViewResult Results(WebpageSearchQuery searchQuery)
        {
            ViewData["results"] = _webpageAdminSearchService.Search(searchQuery);
            return PartialView(searchQuery);
        }
    }
}