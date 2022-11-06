using AntDesign.Charts;
using Humanizer;
using PrackyASusarny.Data.ServiceInterfaces;

namespace PrackyASusarny.Features.ChartPages;

public static class ChartsUtils
{
    private static readonly string DescriptionFormat = "{0} {1} for {2}";

    public static string GetChartDescription(string chartUsage, string entity,
        (LocalDate? startDate, LocalDate? endDate) dateSpan, ILocalizationService loc)
    {
        string dateSpanStr;
        if (dateSpan.startDate != null && dateSpan.endDate != null)
            dateSpanStr = $"at {loc.GetLocalizedDate(dateSpan.startDate)} - {loc.GetLocalizedDate(dateSpan.endDate)}";
        else if (dateSpan.startDate != null)
            dateSpanStr = $"at {loc.GetLocalizedDate(dateSpan.startDate)}";
        else if (dateSpan.endDate != null)
            dateSpanStr = $"at {loc.GetLocalizedDate(dateSpan.endDate)}";
        else
            dateSpanStr = "";
        return string.Format(DescriptionFormat, chartUsage, dateSpanStr, entity);
    }

    public static ColumnConfig CreateColumnConfig(string title, string description, string xfield, string yfield)
    {
        return new ColumnConfig
        {
            Title = new Title
            {
                Visible = true,
                Text = title
            },
            Description = new Description
            {
                Visible = true,
                Text = description
            },
            ForceFit = true,
            Padding = "auto",
            XField = "x",
            YField = "y",
            Label = new ColumnViewConfigLabel
            {
                Visible = true,
                Style = new TextStyle
                {
                    FontSize = 12,
                    FontWeight = 600,
                    Opacity = 0.6
                }
            },
            Meta = new
            {
                x = new
                {
                    Alias = xfield.Pascalize()
                },
                y = new
                {
                    Alias = yfield.Pascalize()
                }
            }
        };
    }
}