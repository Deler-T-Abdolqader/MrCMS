﻿using Microsoft.AspNetCore.Mvc;
using MrCMS.Services.FileMigration;
using MrCMS.Web.Admin.Infrastructure.Helpers;
using MrCMS.Website.Controllers;

namespace MrCMS.Web.Admin.Controllers
{
    public class FileMigrationController : MrCMSAdminController
    {
        private readonly IFileMigrationService _fileMigrationService;

        public FileMigrationController(IFileMigrationService fileMigrationService)
        {
            _fileMigrationService = fileMigrationService;
        }

        public PartialViewResult Show()
        {
            return PartialView();
        }

        [HttpPost]
        public ActionResult Migrate()
        {
            FileMigrationResult result = _fileMigrationService.MigrateFiles();

            if (result.MigrationRequired)
                TempData.SuccessMessages().Add(result.Message);
            else
                TempData.InfoMessages().Add(result.Message);

            return RedirectToAction("FileSystem", "Settings");
        }
    }
}