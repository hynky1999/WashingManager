using AntDesign.Charts;
using Humanizer;
using PrackyASusarny.Data.ServiceInterfaces;

namespace PrackyASusarny.Features.Charts;

public static class ChartsUtils
{

    public static string GetChartDescription(string chartUsage, string entity,
        (LocalDate? startDate, LocalDate? endDate) dateSpan,
        ILocalizationService loc)
    {
        var dateSpanStrs = new[] {loc[dateSpan.startDate], loc[dateSpan.endDate]};
        var dateStr = String.Join("-", dateSpanStrs);
        string? at = null;
        if (dateStr != "")
        {
            at = loc["at"];
        }

        return $"{loc[chartUsage]} {at} {dateStr} {loc["for"]} {loc[entity]}";
    }

    public static ColumnConfig CreateColumnConfig(string title,
        string description, string xfield, string yfield, ILocalizationService loc)
    {
        return new ColumnConfig
        {
            Title = new Title
            {
                Visible = true,
                Text = loc[title]
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
                    Alias = loc[xfield.Pascalize()]
                },
                y = new
                {
                    Alias = loc[yfield.Pascalize()]
                }
            }
        };
    }
}