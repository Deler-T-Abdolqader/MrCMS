using MrCMS.Entities.Documents;
using X.PagedList;

namespace MrCMS.Web.Admin.Models
{
    public class VersionsModel : AsyncListModel<DocumentVersion>
    {
        public VersionsModel(IPagedList<DocumentVersion> items, int id)
            : base(items, id)
        {
        }
    }
}