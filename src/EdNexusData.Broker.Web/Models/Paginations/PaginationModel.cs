namespace EdNexusData.Broker.Web.Models.Paginations;

public class PaginationModel
{
    public int Page { get; set; } = 1;
    public int Size { get; set; } = 10;
    public string? SortBy { get; set; }
    public string? SortDir { get; set; }
    public bool IsAscending => (SortDir is null || SortDir.Equals("asc", StringComparison.OrdinalIgnoreCase)) ? true : false;
}
