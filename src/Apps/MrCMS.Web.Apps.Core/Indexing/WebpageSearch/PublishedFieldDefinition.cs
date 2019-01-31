using System.Collections.Generic;
using Lucene.Net.Documents;
using MrCMS.Entities.Documents.Web;
using MrCMS.Indexing.Management;

namespace MrCMS.Web.Apps.Core.Indexing.WebpageSearch
{
    public class PublishedFieldDefinition : StringFieldDefinition<WebpageSearchIndexDefinition, Webpage>
    {
        public PublishedFieldDefinition(ILuceneSettingsService luceneSettingsService) : base(luceneSettingsService, "published")
        {
        }

        protected override IEnumerable<string> GetValues(Webpage obj)
        {
            yield return obj.Published.ToString();
        }
    }
}