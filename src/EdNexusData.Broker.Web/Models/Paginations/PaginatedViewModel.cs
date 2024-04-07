namespace EdNexusData.Broker.Web.Models.Paginations;

public class PaginatedViewModel<T>
{
    public IList<T> Items { get; private set; }
    public int Page { get; private set; }
    public int Size { get; private set; }
    public int TotalItems { get; private set; }
    public string SortBy { get; private set; }
    public string SortDir { get; private set; }
    public string SearchBy { get; private set; }
    public bool ShowNext => Page * Size < TotalItems;
    public bool ShowPrevious => Page > 1;
    public int Showing => Page == 1 ? 1 : (Page-1) * Size;
    public int ShowingTo => Math.Min(Page * Size, TotalItems);

    public PaginatedViewModel(
        IEnumerable<T> items,
        int totalItems,
        int page,
        int size,
        string sortBy,
        string sortDir,
        string searchBy
        )
    {
        Items = new List<T>(items);
        TotalItems = totalItems;
        Page = page;
        Size = size;
        SortBy = sortBy;
        SortDir = sortDir;
        SearchBy = searchBy;
    }

    public PaginatedViewModel<T> SetData(IList<T> items)
    {
        Items = new List<T>(items);
        return this;
    }

    public PaginatedViewModel<T> SetTotalItems(int totalItems)
    {
        TotalItems = totalItems;
        return this;
    }
}
