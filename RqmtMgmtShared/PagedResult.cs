namespace RqmtMgmtShared
{
    /// <summary>
    /// Generic paged result container for API responses with pagination metadata.
    /// Provides both the data items and pagination information for client-side navigation.
    /// </summary>
    /// <typeparam name="T">The type of items in the paged result.</typeparam>
    public class PagedResult<T>
    {
        /// <summary>
        /// Gets or sets the items for the current page.
        /// </summary>
        public List<T> Items { get; set; } = new();

        /// <summary>
        /// Gets or sets the current page number (1-based).
        /// </summary>
        public int PageNumber { get; set; }

        /// <summary>
        /// Gets or sets the number of items per page.
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Gets or sets the total number of items across all pages.
        /// </summary>
        public int TotalItems { get; set; }

        /// <summary>
        /// Gets the total number of pages based on TotalItems and PageSize.
        /// </summary>
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);

        /// <summary>
        /// Gets whether there is a previous page available.
        /// </summary>
        public bool HasPreviousPage => PageNumber > 1;

        /// <summary>
        /// Gets whether there is a next page available.
        /// </summary>
        public bool HasNextPage => PageNumber < TotalPages;

        /// <summary>
        /// Gets the 1-based index of the first item on the current page.
        /// </summary>
        public int FirstItemIndex => (PageNumber - 1) * PageSize + 1;

        /// <summary>
        /// Gets the 1-based index of the last item on the current page.
        /// </summary>
        public int LastItemIndex => Math.Min(PageNumber * PageSize, TotalItems);
    }

    /// <summary>
    /// Parameters for paginated requests.
    /// Provides standardized pagination parameters with validation and defaults.
    /// </summary>
    public class PaginationParameters
    {
        private int _pageNumber = 1;
        private int _pageSize = 20;

        /// <summary>
        /// Gets or sets the page number (1-based). Minimum value is 1.
        /// </summary>
        public int PageNumber 
        { 
            get => _pageNumber; 
            set => _pageNumber = value < 1 ? 1 : value; 
        }

        /// <summary>
        /// Gets or sets the page size. Must be between 1 and 100.
        /// </summary>
        public int PageSize 
        { 
            get => _pageSize; 
            set => _pageSize = value < 1 ? 1 : value > 100 ? 100 : value; 
        }

        /// <summary>
        /// Gets or sets the search term for filtering results.
        /// </summary>
        public string? SearchTerm { get; set; }

        /// <summary>
        /// Gets or sets the field to sort by.
        /// </summary>
        public string? SortBy { get; set; }

        /// <summary>
        /// Gets or sets whether to sort in descending order. Default is false (ascending).
        /// </summary>
        public bool SortDescending { get; set; } = false;

        /// <summary>
        /// Gets or sets the project ID to filter results by. When provided, only results for this project are returned.
        /// </summary>
        public int? ProjectId { get; set; }
    }
}