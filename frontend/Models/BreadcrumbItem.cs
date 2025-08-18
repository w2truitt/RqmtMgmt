namespace frontend.Models
{
    public class BreadcrumbItem
    {
        public string Title { get; set; } = string.Empty;
        public string? Icon { get; set; }
        public string? Url { get; set; }
        public bool IsActive { get; set; }
    }
}
