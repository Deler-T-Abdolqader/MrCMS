using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HtmlAgilityPack;
using MrCMS.Entities.Documents.Web;
using MrCMS.Web.Areas.Admin.Models.SEOAnalysis;

namespace MrCMS.Web.Areas.Admin.Services.SEOAnalysis
{
    public abstract class BaseSEOAnalysisFacetProvider : ISEOAnalysisFacetProvider
    {
        public abstract IAsyncEnumerable<SEOAnalysisFacet> GetFacets(Webpage webpage, HtmlNode document, string analysisTerm);

        protected SEOAnalysisFacet GetFacet(string name, SEOAnalysisStatus status, int importance, params string[] messages)
        {
            return new SEOAnalysisFacet
                   {
                       Name = name,
                       Status = status,
                       Messages = messages.ToList(),
                       Importance = importance
                   };
        }
        protected Task<SEOAnalysisFacet> GetFacet(string name, SEOAnalysisStatus status, params string[] messages)
        {
            return Task.FromResult(new SEOAnalysisFacet
            {
                Name = name,
                Status = status,
                Messages = messages.ToList()
            });
        }
    }
}