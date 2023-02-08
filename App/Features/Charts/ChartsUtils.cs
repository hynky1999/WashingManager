using AntDesign.Charts;
using App.Data.ServiceInterfaces;

namespace App.Features.Charts;

/// <summary>
/// Utils for creating charts configuration
/// </summary>
public static class ChartsUtils
{
    /// <summary>
    /// Creates a localized chart description based on name, entity that chart is based on and date range
    /// </summary>
    /// <param name="chartUsage">name</param>
    /// <param name="entity"></param>
    /// <param name="dateSpan"></param>
    /// <param name="loc"></param>
    /// <returns></returns>
    public static string GetChartDescription(string chartUsage, string entity,
        (LocalDate? startDate, LocalDate? endDate) dateSpan,
        ILocalizationService loc)
    {
        var dateSpanStrs = new[]
            {loc[dateSpan.startDate], loc[dateSpan.endDate]};
        var dateStr = String.Join(dateSpanStrs.All(d => d != null) ? "-" : "",
            dateSpanStrs);
        if (dateStr != "")
        {
            dateStr = $" {loc["at"]} {dateStr}";
        }

        return $"{loc[chartUsage]}{dateStr} {loc["for"]} {loc[entity]}";
    }

    /// <summary>
    /// Creates a localized column configuration for a chart based on title, description and axis names
    /// </summary>
    /// <param name="title"></param>
    /// <param name="description"></param>
    /// <param name="xfield">x axis</param>
    /// <param name="yfield">y axis</param>
    /// <param name="loc"></param>
    /// <returns></returns>
    public static ColumnConfig CreateColumnConfig(string title,
        string description, string xfield, string yfield,
        ILocalizationService loc)
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
                    Alias = loc[xfield]
                },
                y = new
                {
                    Alias = loc[yfield]
                }
            }
        };
    }
}