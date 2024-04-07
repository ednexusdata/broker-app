namespace EdNexusData.Broker.Web.Models.Paginations;

public class PaginationModel
{
    public int Page { get; set; } = 1;
    public int Size { get; set; } = 10;
    public string SortBy { get; set; } = "Id";
    public string SortDir { get; set; } = "desc";
    public bool IsAscending => SortDir.Equals("asc", StringComparison.OrdinalIgnoreCase);
}
