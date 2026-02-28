namespace PanOpticon.Models;

public record PageItem(int Id, string Name);

public record PagesResponse(IReadOnlyList<PageItem> Data);

public record WidgetsResponse(
    IReadOnlyList<string> WidgetsNames,
    string CompanyTimeZone,
    string PageName,
    int ProductId);

public record FilterValue(string Label, object Value);

public record FilterItem(string FilterKey, string FilterLabel, IReadOnlyList<FilterValue> Values);

public record FiltersResponse(IReadOnlyList<FilterItem> Filters, string PageName);
