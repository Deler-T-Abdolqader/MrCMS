﻿using Microsoft.AspNetCore.Mvc;
using MrCMS.Website;
using MrCMS.Website.Controllers;

namespace MrCMS.Web.Admin.Controllers
{
    public class ClearCachesController : MrCMSAdminController
    {
        private readonly IClearCachesService _clearCachesService;

        public ClearCachesController(IClearCachesService clearCachesService)
        {
            _clearCachesService = clearCachesService;
        }

        [HttpGet]
        public ViewResult Index()
        {
            ViewData["cleared"] = TempData["cleared"];
            return View();
        }

        [HttpPost]
        [ActionName("Index")]
        public RedirectToActionResult Index_POST()
        {
            _clearCachesService.ClearCache();
            TempData["cleared"] = true;
            return RedirectToAction("Index");
        }
    }
}