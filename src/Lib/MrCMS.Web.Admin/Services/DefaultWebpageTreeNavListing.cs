using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using MrCMS.Entities.Documents;
using MrCMS.Entities.Documents.Web;
using MrCMS.Helpers;
using MrCMS.Services;
using MrCMS.Web.Admin.Models;
using NHibernate;
using NHibernate.Criterion;

namespace MrCMS.Web.Admin.Services
{
    public class DefaultWebpageTreeNavListing : IWebpageTreeNavListing
    {
        private readonly ISession _session;
        private readonly ITreeNavService _treeNavService;
        private readonly IDocumentMetadataService _documentMetadataService;
        private readonly IUrlHelper _urlHelper;
        private readonly IValidWebpageChildrenService _validWebpageChildrenService;

        public DefaultWebpageTreeNavListing(IValidWebpageChildrenService validWebpageChildrenService, ISession session,
            IUrlHelper urlHelper, ITreeNavService treeNavService,IDocumentMetadataService documentMetadataService)
        {
            _validWebpageChildrenService = validWebpageChildrenService;
            _session = session;
            _urlHelper = urlHelper;
            _treeNavService = treeNavService;
            _documentMetadataService = documentMetadataService;
        }

        public AdminTree GetTree(int? id)
        {
            Webpage parent = id.HasValue ? _session.Get<Webpage>(id) : null;
            var adminTree = new AdminTree
            {
                RootContoller = "Webpage",
                IsRootRequest = parent == null
            };
            int maxChildNodes = parent == null ? 1000 : _documentMetadataService.GetMetadata(parent).MaxChildNodes;
            IQueryOver<Webpage, Webpage> query = GetQuery(parent);

            int rowCount = GetRowCount(query);
            query.Take(maxChildNodes).Cacheable().List().ForEach(doc =>
            {
                DocumentMetadata documentMetadata = _documentMetadataService.GetMetadata(doc);
                var node = new AdminTreeNode
                {
                    Id = doc.Id,
                    ParentId = doc.ParentId,
                    Name = doc.Name,
                    IconClass = documentMetadata.IconClass,
                    NodeType = "Webpage",
                    Type = documentMetadata.Type.FullName,
                    HasChildren = _treeNavService.WebpageHasChildren(doc.Id),
                    Sortable = documentMetadata.Sortable,
                    CanAddChild = _validWebpageChildrenService.AnyValidWebpageDocumentTypes(doc),
                    IsPublished = doc.Published,
                    RevealInNavigation = doc.RevealInNavigation,
                    Url = _urlHelper.Action("Edit", "Webpage", new {id = doc.Id})
                };
                adminTree.Nodes.Add(node);
            });
            if (rowCount > maxChildNodes)
            {
                adminTree.Nodes.Add(new AdminTreeNode
                {
                    IconClass = "fa fa-plus",
                    IsMoreLink = true,
                    ParentId = id,
                    Name = (rowCount - maxChildNodes) + " More",
                    Url = _urlHelper.Action("Search", "WebpageSearch", new {parentId = id}),
                });
            }
            return adminTree;
        }

        public bool HasChildren(int id)
        {
            var parent = _session.Get<Webpage>(id);
            IQueryOver<Webpage, Webpage> query = GetQuery(parent);
            return GetRowCount(query) > 0;
        }

        private static int GetRowCount(IQueryOver<Webpage, Webpage> query)
        {
            return query.Cacheable().RowCount();
        }

        private IQueryOver<Webpage, Webpage> GetQuery(Webpage parent)
        {
            IQueryOver<Webpage, Webpage> query = _session.QueryOver<Webpage>();
            if (parent != null)
            {
                query = query.Where(x => x.Parent.Id == parent.Id);
                DocumentMetadata metaData = _documentMetadataService.GetMetadata(parent);
                query = ApplySort(metaData, query);
            }
            else
            {
                query = query.Where(x => x.Parent == null);
                query = query.OrderBy(x => x.DisplayOrder).Asc;
            }
            return query;
        }

        private static IQueryOver<Webpage, Webpage> ApplySort(DocumentMetadata metaData,
            IQueryOver<Webpage, Webpage> query)
        {
            switch (metaData.SortBy)
            {
                case SortBy.DisplayOrder:
                    query = query.OrderBy(webpage => webpage.DisplayOrder).Asc;
                    break;
                case SortBy.DisplayOrderDesc:
                    query = query.OrderBy(webpage => webpage.DisplayOrder).Desc;
                    break;
                case SortBy.PublishedOn:
                    query =
                        query.OrderBy(
                            Projections.Conditional(
                                Restrictions.IsNull(Projections.Property<Webpage>(x => x.PublishOn)),
                                Projections.Constant(1), Projections.Constant(0)))
                            .Desc.ThenBy(webpage => webpage.PublishOn)
                            .Asc;
                    break;
                case SortBy.PublishedOnDesc:
                    query =
                        query.OrderBy(
                            Projections.Conditional(
                                Restrictions.IsNull(Projections.Property<Webpage>(x => x.PublishOn)),
                                Projections.Constant(1), Projections.Constant(0)))
                            .Desc.ThenBy(webpage => webpage.PublishOn)
                            .Desc;
                    break;
                case SortBy.CreatedOn:
                    query = query.OrderBy(webpage => webpage.CreatedOn).Asc;
                    break;
                case SortBy.CreatedOnDesc:
                    query = query.OrderBy(webpage => webpage.CreatedOn).Desc;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return query;
        }
    }
}