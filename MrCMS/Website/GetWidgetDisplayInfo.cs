using System.Collections.Generic;
using MrCMS.Entities.Documents.Layout;
using MrCMS.Entities.Documents.Web;
using MrCMS.Helpers;

namespace MrCMS.Website
{
    public class GetWidgetDisplayInfo : IGetWidgetDisplayInfo
    {
        private readonly IGetWidgetsForAreas _getWidgetsForAreas;
        private readonly IMapWidgetDisplayInfo _mapWidgetDisplayInfo;

        public GetWidgetDisplayInfo(
            IGetWidgetsForAreas getWidgetsForAreas,
            IMapWidgetDisplayInfo mapWidgetDisplayInfo
        )
        {
            _getWidgetsForAreas = getWidgetsForAreas;
            _mapWidgetDisplayInfo = mapWidgetDisplayInfo;
        }

        public IDictionary<string, WidgetDisplayInfo> GetWidgets(Layout layout, Webpage webpage)
        {
            var layoutAreas = layout.GetLayoutAreas();

            var widgetData = _getWidgetsForAreas.GetWidgets(layoutAreas, webpage);

            return _mapWidgetDisplayInfo.MapInfo(widgetData);
        }
    }
}