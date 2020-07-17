using Microsoft.AspNetCore.Mvc.ViewFeatures;
using MrCMS.Entities.Documents;
using MrCMS.Entities.Documents.Web;
using MrCMS.Helpers;
using MrCMS.Services;
using MrCMS.Web.Admin.Infrastructure.Routing;
using MrCMS.Website.Auth;
using System.Collections.Generic;
using System.Linq;
using MrCMS.Web.Admin.Models;

namespace MrCMS.Web.Admin.Services
{
    public class WebpageBaseViewDataService : IWebpageBaseViewDataService
    {
        private readonly IGetValidPageTemplatesToAdd _getValidPageTemplatesToAdd;
        private readonly IGetCurrentUser _getCurrentUser;
        private readonly IAccessChecker _accessChecker;
        private readonly IGetAdminActionDescriptor _getDescriptor;
        private readonly IValidWebpageChildrenService _validWebpageChildrenService;

        public WebpageBaseViewDataService(IValidWebpageChildrenService validWebpageChildrenService,
            IGetValidPageTemplatesToAdd getValidPageTemplatesToAdd,
            IGetCurrentUser getCurrentUser,
            IAccessChecker accessChecker,
            IGetAdminActionDescriptor getDescriptor)
        {
            _validWebpageChildrenService = validWebpageChildrenService;
            _getValidPageTemplatesToAdd = getValidPageTemplatesToAdd;
            _getCurrentUser = getCurrentUser;
            _accessChecker = accessChecker;
            _getDescriptor = getDescriptor;
        }

        public void SetAddPageViewData(ViewDataDictionary viewData, Webpage parent)
        {
            viewData["parent"] = parent;

            IOrderedEnumerable<DocumentMetadata> validWebpageDocumentTypes = _validWebpageChildrenService
                .GetValidWebpageDocumentTypes(parent,
                    metadata => _accessChecker.CanAccess(_getDescriptor.GetDescriptor("Webpage", "Add"), _getCurrentUser.Get()))
                .OrderBy(metadata => metadata.DisplayOrder);

            var templates = _getValidPageTemplatesToAdd.Get(validWebpageDocumentTypes);

            var documentTypeToAdds = new List<DocumentTypeToAdd>();

            foreach (DocumentMetadata type in validWebpageDocumentTypes)
            {
                if (templates.Any(template => template.PageType == type.Type.FullName))
                {
                    documentTypeToAdds.Add(new DocumentTypeToAdd
                    {
                        Type = type,
                        DisplayName = string.Format("Default {0}", type.Name)
                    });
                    DocumentMetadata typeCopy = type;
                    documentTypeToAdds.AddRange(
                        templates.Where(template => template.PageType == typeCopy.Type.FullName)
                            .Select(pageTemplate => new DocumentTypeToAdd
                            {
                                Type = type,
                                DisplayName = pageTemplate.Name,
                                TemplateId = pageTemplate.Id
                            }));
                }
                else
                {
                    documentTypeToAdds.Add(new DocumentTypeToAdd { Type = type, DisplayName = type.Name });
                }
            }

            viewData["DocumentTypes"] = documentTypeToAdds;
        }

        public void SetEditPageViewData(ViewDataDictionary viewData, Webpage page)
        {
            DocumentMetadata documentMetadata = page.GetMetadata();
            if (documentMetadata != null)
            {
                viewData["EditView"] = documentMetadata.EditPartialView;
            }
        }
    }
}