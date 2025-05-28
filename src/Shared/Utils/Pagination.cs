namespace Shared.Utils
{    /// <summary>
    /// Represents pagination information for a dataset.
    /// </summary>
    /// <param name="Total">The total number of items in the dataset.</param>
    /// <param name="PageIndex">The current page index (1-based). Defaults to 1.</param>
    /// <param name="PageSize">The number of items per page. Defaults to 10.</param>
    public record Pagination(int Total = 0, int PageIndex = 1, int PageSize = 10)
    {
        public int Total { get; set; } = Total;
        public int PageIndex { get; init; } = PageIndex < 1 ? 1 : PageIndex;
        public int PageSize { get; init; } = PageSize;

        /// <summary>
        /// Calculates the total number of pages based on the total items and page size.
        /// </summary>
        public int TotalPages => (int)Math.Ceiling((double)Total / PageSize);

        /// <summary>
        /// Gets the starting index for the current page.
        /// </summary>
        public int StartIndex => (PageIndex - 1) * PageSize;

        /// <summary>
        /// Gets the ending index for the current page.
        /// </summary>
        public int EndIndex => Math.Min(StartIndex + PageSize - 1, Total - 1);
    }
}
