namespace SupportTicketAPI.Models
{
    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new();

        public int PageNumber { get; set; }

        public int PageSize { get; set; }

        public int TotalCount { get; set; }

        public int TotalPages
        {
            get
            {
                if (PageSize <= 0)
                {
                    return 0;
                }

                return (int)Math.Ceiling((double)TotalCount / PageSize);
            }
        }
    }
}