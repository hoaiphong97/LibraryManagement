namespace LibraryManagement.DTOs
{
    public class ImportResultDto
    {
        public int TotalRows { get; set; }
        public int SuccessCount { get; set; }
        public int SkippedCount { get; set; }
        public int TotalBooksCreated { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<string> SkippedBooks { get; set; } = new();
    }
}
