using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc.Rendering;
using MrCMS.Data;
using MrCMS.Entities.Multisite;
using MrCMS.Entities.Resources;
using MrCMS.Helpers;
using MrCMS.Services.Resources;
using MrCMS.Settings;
using MrCMS.Web.Areas.Admin.Models;
using X.PagedList;
using IConfigurationProvider = MrCMS.Settings.IConfigurationProvider;

namespace MrCMS.Web.Areas.Admin.Services
{
    public class StringResourceAdminService : IStringResourceAdminService
    {
        private const string DefaultLanguage = "Default";
        private readonly IStringResourceProvider _provider;
        private readonly IConfigurationProvider _configurationProvider;
        private readonly IMapper _mapper;
        private readonly IGlobalRepository<StringResource> _repository;
        private readonly IGlobalRepository<Site> _siteRepository;

        public StringResourceAdminService(IStringResourceProvider provider, IConfigurationProvider configurationProvider,
            IGlobalRepository<StringResource> repository,
            IGlobalRepository<Site> siteRepository, // todo - refactor this out

            IMapper mapper)
        {
            _provider = provider;
            _configurationProvider = configurationProvider;
            _repository = repository;
            _siteRepository = siteRepository;
            _mapper = mapper;
        }

        public async Task<IPagedList<StringResource>> Search(StringResourceSearchQuery searchQuery)
        {
            IEnumerable<StringResource> resources =
                _provider.AllResources.GetResourcesByKeyAndValue(searchQuery);
            if (searchQuery.Language == DefaultLanguage)
            {
                resources = resources.Where(resource => resource.UICulture == null);
            }
            else if (!string.IsNullOrWhiteSpace(searchQuery.Language))
            {
                resources = resources.Where(resource => resource.UICulture == searchQuery.Language);
            }

            if (searchQuery.SiteId.HasValue)
            {
                if (searchQuery.SiteId == -1)
                {
                    resources = resources.Where(resource => resource.Site == null);
                }
                else if (searchQuery.SiteId > 0)
                {
                    resources =
                        resources.Where(resource => resource.Site != null && resource.Site.Id == searchQuery.SiteId);
                }
            }

            var siteSettings = await _configurationProvider.GetSiteSettings<SiteSettings>();
            return new PagedList<StringResource>(resources.OrderBy(resource => StringResourceExtensions.GetDisplayKey(resource.Key)), searchQuery.Page,
                siteSettings.DefaultPageSize);
        }

        public void Add(AddStringResourceModel model)
        {
            var resource = _mapper.Map<StringResource>(model);
            _provider.AddOverride(resource);
        }

        public StringResource GetResource(int id)
        {
            return _repository.GetDataSync(id);
        }

        public UpdateStringResourceModel GetEditModel(StringResource resource)
        {
            return _mapper.Map<UpdateStringResourceModel>(resource);
        }

        public void Update(UpdateStringResourceModel model)
        {
            var resource = GetResource(model.Id);
            _mapper.Map(model, resource);
            _provider.Update(resource);
        }

        public void Delete(int id)
        {
            var resource = _repository.LoadSync(id);
            _provider.Delete(resource);
        }

        public async Task<List<SelectListItem>> GetLanguageOptions(string key, int? siteId)
        {
            List<CultureInfo> cultureInfos = CultureInfo.GetCultures(CultureTypes.SpecificCultures).ToList();
            IEnumerable<string> languages = _provider.GetOverriddenLanguages(key, siteId);
            cultureInfos.RemoveAll(info => languages.Contains(info.Name));
            var siteSettings = await _configurationProvider.GetSiteSettings<SiteSettings>();
            return cultureInfos.OrderBy(info => info.DisplayName)
                .BuildSelectItemList(info => info.DisplayName, info => info.Name,
                    info => info.Name == siteSettings.UICulture,
                    SelectListItemHelper.EmptyItem("Select a culture..."));
        }

        public List<SelectListItem> SearchLanguageOptions()
        {
            List<CultureInfo> cultureInfos = CultureInfo.GetCultures(CultureTypes.SpecificCultures).ToList();
            IEnumerable<string> languages = _provider.GetOverriddenLanguages();
            cultureInfos = cultureInfos.FindAll(info => languages.Contains(info.Name));

            List<SelectListItem> selectListItems = cultureInfos.OrderBy(info => info.DisplayName)
                .BuildSelectItemList(info => info.DisplayName, info => info.Name, emptyItem: null);

            selectListItems.Insert(0, new SelectListItem { Text = "Any", Value = "" });
            selectListItems.Insert(1, new SelectListItem { Text = DefaultLanguage, Value = DefaultLanguage });
            return selectListItems;
        }

        public AddStringResourceModel GetNewResource(string key, int? id)
        {
            string value =
                _provider.AllResources.Where(x => x.Key == key && x.Site == null && x.UICulture == null)
                    .Select(resource => resource.Value)
                    .FirstOrDefault();
            return new AddStringResourceModel { Key = key, SiteId = id, Value = value };
        }

        public List<SelectListItem> ChooseSiteOptions(ChooseSiteParams chooseSiteParams)
        {
            IEnumerable<StringResource> resourcesByKey = _provider.AllResources.Where(x => x.Key == chooseSiteParams.Key);
            List<Site> sites = GetAllSites();

            if (!chooseSiteParams.Language)
            {
                resourcesByKey = resourcesByKey.Where(resource => resource.Site != null && resource.UICulture == null);
                sites =
                    sites.Where(site => !resourcesByKey.Select(resource => resource.Site.Id).Contains(site.Id)).ToList();
            }

            return sites
                .BuildSelectItemList(site => site.DisplayName, site => site.Id.ToString(), emptyItem: null);
        }

        public List<SelectListItem> SearchSiteOptions()
        {
            List<SelectListItem> siteOptions = GetAllSites()
                .BuildSelectItemList(site => site.DisplayName, site => site.Id.ToString(), emptyItemText: "All");

            siteOptions.Insert(1, new SelectListItem
            {
                Text = "System Default",
                Value = "-1"
            });

            return siteOptions;
        }

        private List<Site> GetAllSites()
        {
            return _siteRepository.Query()
                .OrderBy(x => x.Name).ToList();
        }
    }
}