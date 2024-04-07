using EdNexusData.Broker.Web.Models.Paginations;

namespace EdNexusData.Broker.Web.Models.Searchables
{
	public class SearchableModelWithPagination : PaginationModel
	{
        public string SearchBy { get; set; } = string.Empty;
    }
}

