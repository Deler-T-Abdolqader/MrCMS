﻿using System;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc.Rendering;
using MrCMS.Entities.Documents.Web;
using MrCMS.Web.Admin.Infrastructure.Models.Tabs;

namespace MrCMS.Web.Admin.Models.WebpageEdit
{
    public class PropertiesTab : AdminTab<Webpage>
    {
        public override int Order
        {
            get { return 0; }
        }

        public override string Name(IServiceProvider serviceProvider, Webpage entity)
        {
            return "Properties";
        }

        public override bool ShouldShow(IServiceProvider serviceProvider, Webpage entity)
        {
            return true;
        }

        public override Type ParentType
        {
            get { return null; }
        }

        public override Type ModelType => typeof(WebpagePropertiesTabViewModel);

        public override string TabHtmlId
        {
            get { return "edit-content"; }
        }

        public override Task RenderTabPane(IHtmlHelper html, IMapper mapper, Webpage webpage)
        {
            return html.RenderPartialAsync("Properties", mapper.Map<WebpagePropertiesTabViewModel>(webpage));
        }
    }
}