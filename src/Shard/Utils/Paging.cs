namespace Shard.Utils
{
    /// <summary>
    /// Represents pagination information for a dataset.
    /// </summary>
    /// <param name="Total">The total number of items in the dataset.</param>
    /// <param name="PageIndex">The current page index (1-based). Defaults to 1.</param>
    /// <param name="PageSize">The number of items per page. Defaults to 10.</param>
    public record Paging(int Total, int PageIndex = 1, int PageSize = 10);
}
