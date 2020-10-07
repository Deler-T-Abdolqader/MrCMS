using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Rendering;
using MrCMS.Apps;
using MrCMS.Entities.Documents.Layout;
using MrCMS.Entities.Documents.Web;
using MrCMS.Helpers;
using MrCMS.Services;
using MrCMS.Settings;
using MrCMS.Web.Admin.Models;
using MrCMS.Website;
using NHibernate;

namespace MrCMS.Web.Admin.Services
{
    public class PageDefaultsAdminService : IPageDefaultsAdminService
    {
        private readonly IGetUrlGeneratorOptions _getUrlGeneratorOptions;
        private readonly IGetLayoutOptions _getLayoutOptions;
        private readonly ISession _session;
        private readonly MrCMSAppContext _appContext;
        private readonly IDocumentMetadataService _documentMetadataService;
        private readonly IConfigurationProvider _configurationProvider;

        public PageDefaultsAdminService(IConfigurationProvider configurationProvider,
            IGetUrlGeneratorOptions getUrlGeneratorOptions, IGetLayoutOptions getLayoutOptions, ISession session,
            MrCMSAppContext appContext,IDocumentMetadataService documentMetadataService)
        {
            _configurationProvider = configurationProvider;
            _getUrlGeneratorOptions = getUrlGeneratorOptions;
            _getLayoutOptions = getLayoutOptions;
            _session = session;
            _appContext = appContext;
            _documentMetadataService = documentMetadataService;
        }


        public List<PageDefaultsInfo> GetAll()
        {
            var layoutOptions = _getLayoutOptions.Get();
            var settings = _configurationProvider.GetSiteSettings<PageDefaultsSettings>();
            var webpages = TypeHelper.GetAllConcreteMappedClassesAssignableFrom<Webpage>();
            return (from key in webpages
                    select new PageDefaultsInfo
                    {
                        DisplayName = GetDisplayName(key),
                        TypeName = key.FullName,
                        GeneratorDisplayName = settings.GetGeneratorType(key).Name.BreakUpString(),
                        LayoutName = GetLayoutName(layoutOptions, key),
                        CacheEnabled = key.GetCustomAttribute<WebpageOutputCacheableAttribute>(false) == null
                            ? CacheEnabledStatus.Unavailable
                            : settings.CacheDisabled(key)
                                ? CacheEnabledStatus.Disabled
                                : CacheEnabledStatus.Enabled
                    }).ToList();
        }

        private string GetDisplayName(Type key)
        {
            var appName = _appContext.Types.ContainsKey(key) ? _appContext.Types[key].Name : "System"; 
            return $"{_documentMetadataService.GetMetadata(key).Name} ({appName})";
        }

        private string GetLayoutName(List<SelectListItem> layoutOptions, Type key)
        {
            var settings = _configurationProvider.GetSiteSettings<PageDefaultsSettings>();
            var layoutId = settings.GetLayoutId(key);
            if (!layoutId.HasValue || layoutOptions.All(item => item.Value != layoutId.ToString()))
            {
                var siteSettings = _configurationProvider.GetSiteSettings<SiteSettings>();
                var systemDefaultLayout = _session.Get<Layout>(siteSettings.DefaultLayoutId);
                return $"System Default ({systemDefaultLayout.Name})";
            }
            return _session.Get<Layout>(layoutId.Value).Name;
        }

        public List<SelectListItem> GetUrlGeneratorOptions(Type type)
        {
            var settings = _configurationProvider.GetSiteSettings<PageDefaultsSettings>();
            var currentGeneratorType = settings.GetGeneratorType(type);
            return _getUrlGeneratorOptions.Get(type, currentGeneratorType);
        }

        public List<SelectListItem> GetLayoutOptions()
        {
            return _getLayoutOptions.Get();
        }

        public DefaultsInfo GetInfo(Type type)
        {
            var settings = _configurationProvider.GetSiteSettings<PageDefaultsSettings>();
            return new DefaultsInfo
            {
                PageTypeName = type.FullName,
                PageTypeDisplayName = GetDisplayName(type),
                GeneratorTypeName = settings.GetGeneratorType(type).FullName,
                LayoutId = settings.GetLayoutId(type)
            };
        }

        public void SetDefaults(DefaultsInfo info)
        {
            var settings = _configurationProvider.GetSiteSettings<PageDefaultsSettings>();
            settings.UrlGenerators[info.PageTypeName] = info.GeneratorTypeName;
            settings.Layouts[info.PageTypeName] = info.LayoutId;
            _configurationProvider.SaveSettings(settings);
        }

        public void EnableCache(string typeName)
        {
            var settings = _configurationProvider.GetSiteSettings<PageDefaultsSettings>();
            settings.DisableCaches.Remove(typeName);
            _configurationProvider.SaveSettings(settings);
        }

        public void DisableCache(string typeName)
        {
            var settings = _configurationProvider.GetSiteSettings<PageDefaultsSettings>();
            settings.DisableCaches.Add(typeName);
            _configurationProvider.SaveSettings(settings);
        }
    }
}