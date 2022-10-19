namespace PrackyASusarny.Features.ChartsPage;

public static class ChartsUtils
{
    private static readonly string DescriptionFormat = "{0} {1} for {2}";

    public static string GetChartDescription(string chartUsage, string entity, int id,
        (DateTime? startDate, DateTime? endDate) dateSpan)
    {
        string entityStr;
        if (id == 0)
            entityStr = $"all {entity}s";
        else
            entityStr = $"{entity} #{id}";

        string dateSpanStr;
        if (dateSpan.startDate != null && dateSpan.endDate != null)
            dateSpanStr = $"at {dateSpan.startDate:dd/mm/yyyy} - {dateSpan.endDate:dd/mm/yyyy}";
        else if (dateSpan.startDate != null)
            dateSpanStr = $"at {dateSpan.startDate:dd/mm/yyyy}";
        else if (dateSpan.endDate != null)
            dateSpanStr = $"at {dateSpan.startDate:dd/mm/yyyy}";
        else
            dateSpanStr = "";
        return string.Format(DescriptionFormat, chartUsage, dateSpanStr, entityStr);
    }
}