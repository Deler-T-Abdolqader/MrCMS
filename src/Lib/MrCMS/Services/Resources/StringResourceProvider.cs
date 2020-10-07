using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Extensions.Logging;
using MrCMS.DbConfiguration;
using MrCMS.Entities.Multisite;
using MrCMS.Entities.Resources;
using MrCMS.Helpers;
using NHibernate;

namespace MrCMS.Services.Resources
{
    public class StringResourceProvider : IStringResourceProvider
    {
        private static Dictionary<string, HashSet<StringResource>> _allResources;

        private static readonly Dictionary<int, Dictionary<string, HashSet<StringResource>>> _resourcesBySite =
            new Dictionary<int, Dictionary<string, HashSet<StringResource>>>();


        private static readonly object LockObject = new object();
        private readonly IGetCurrentUserCultureInfo _getCurrentUserCultureInfo;
        private readonly ILogger<StringResourceProvider> _logger;
        private readonly ISession _session;
        private readonly Site _site;
        private bool _retryingAllResources;

        public StringResourceProvider(ISession session, Site site, IGetCurrentUserCultureInfo getCurrentUserCultureInfo,
            ILogger<StringResourceProvider> logger)
        {
            _session = session;
            _site = site;
            _getCurrentUserCultureInfo = getCurrentUserCultureInfo;
            _logger = logger;
        }

        private IEnumerable<StringResource> AllStringResources
        {
            get { return AllResources.SelectMany(pair => pair.Value); }
        }

        // retry added to try and help mitigate issues with duplicates being added and causing errors
        private Dictionary<string, HashSet<StringResource>> AllResources
        {
            get
            {
                var allResources =
                    _allResources = _allResources ?? GetAllResourcesFromDb();
                if (!allResources.Any())
                {
                    try
                    {
                        if (!_retryingAllResources)
                        {
                            _logger.Log(LogLevel.Information, "Resource list empty");
                        }
                    }
                    catch
                    {
                        // ignored
                    }

                    if (!_retryingAllResources)
                    {
                        _retryingAllResources = true;
                        ResetResourceCache();
                        return AllResources;
                    }
                }

                return allResources;
            }
        }


        public Dictionary<string, HashSet<StringResource>> ResourcesForSite
        {
            get
            {
                return
                    _resourcesBySite.ContainsKey(_site.Id)
                        ? _resourcesBySite[_site.Id]
                        : _resourcesBySite[_site.Id] =
                            AllResources.Keys.ToDictionary(s => s,
                                s =>
                                    new HashSet<StringResource>(AllResources[s].Where(
                                            resource => resource.Site == null || resource.Site.Id == _site.Id)
                                        .OrderByDescending(resource => resource.Site != null ? 1 : 0)),
                                StringComparer.OrdinalIgnoreCase);
            }
        }

        public string GetValue(string key, string defaultValue = null)
        {
            return GetValueForCulture(key, _getCurrentUserCultureInfo.Get(), defaultValue);
        }

        public string GetValueForCulture(string key, CultureInfo cultureInfo, string defaultValue = null)
        {
            //using (MiniProfiler.Current.Step(string.Format("Getting resource - {0}", key)))
            {
                lock (LockObject)
                {
                    string currentUserCulture;
                    //using (MiniProfiler.Current.Step("culture check"))
                    {
                        currentUserCulture = cultureInfo.Name;
                    }

                    if (ResourcesForSite.ContainsKey(key))
                    {
                        var resources = ResourcesForSite[key];
                        //using (MiniProfiler.Current.Step("Check for language"))
                        {
                            var languageValue =
                                resources.FirstOrDefault(
                                    resource => resource.UICulture == currentUserCulture);
                            if (languageValue != null)
                                return languageValue.Value;
                        }

                        //using (MiniProfiler.Current.Step("Check for default"))
                        {
                            var existingDefault =
                                resources.FirstOrDefault(resource => resource.UICulture == null);
                            if (existingDefault != null)
                                return existingDefault.Value;
                        }
                    }

                    //using (MiniProfiler.Current.Step("default resource"))
                    {
                        var defaultResource = new StringResource
                        {
                            Key = key,
                            Value = defaultValue ?? key,
                            //UICulture = currentUserCulture
                        };
                        _session.Transact(session => session.Save(defaultResource));
                        //AllResources[key] = new HashSet<StringResource> {defaultResource};
                        ResetResourceCache();
                        return defaultResource.Value;
                    }
                }
            }
        }

        public IEnumerable<string> GetOverriddenLanguages()
        {
            return
                ResourcesForSite.SelectMany(x => x.Value)
                    .Select(resource => resource.UICulture)
                    .Distinct()
                    .Where(s => !string.IsNullOrWhiteSpace(s));
        }

        public IEnumerable<string> GetOverriddenLanguages(string key, int? siteId)
        {
            if (ResourcesForSite.ContainsKey(key))
            {
                var stringResources = ResourcesForSite[key];
                stringResources = siteId == null
                    ? stringResources.FindAll(resource => resource.Site == null)
                    : stringResources.FindAll(resource => resource.Site != null && resource.Site.Id == siteId);
                return stringResources
                    .Select(resource => resource.UICulture)
                    .Where(s => !string.IsNullOrWhiteSpace(s));
            }

            return new HashSet<string>();
        }

        public void Insert(StringResource resource)
        {
            lock (LockObject)
            {
                _session.Transact(session => session.Save(resource));
                ResetResourceCache();
            }
        }

        public void AddOverride(StringResource resource)
        {
            lock (LockObject)
            {
                if (resource.UICulture == null && resource.Site == null)
                    return;
                _session.Transact(session => session.Save(resource));
                ResetResourceCache();
            }
        }

        public void Update(StringResource resource)
        {
            lock (LockObject)
            {
                _session.Transact(session => session.Update(resource));
                ResetResourceCache();
            }
        }

        public void Delete(StringResource resource)
        {
            lock (LockObject)
            {
                _session.Transact(session => session.Delete(resource));
                ResetResourceCache();
            }
        }

        IEnumerable<StringResource> IStringResourceProvider.AllResources => AllStringResources;

        public void ClearCache()
        {
            ResetResourceCache();
        }

        private void ResetResourceCache()
        {
            _allResources = null;
            _resourcesBySite.Clear();
        }

        private Dictionary<string, HashSet<StringResource>> GetAllResourcesFromDb()
        {
            lock (LockObject)
            {
                using (new SiteFilterDisabler(_session))
                {
                    var allResourcesFromDb =
                        _session.QueryOver<StringResource>().Cacheable().List().ToHashSet();
                    var groupBy =
                        allResourcesFromDb.GroupBy(resource => resource.Key,
                            StringComparer.OrdinalIgnoreCase);
                    return groupBy.ToDictionary(grouping => grouping.Key, grouping => grouping.ToHashSet());
                }
            }
        }
    }
}