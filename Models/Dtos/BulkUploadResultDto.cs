namespace visitor_admin.Models.Dtos
{
    public class BulkUploadResultDto
    {
        public int TotalRows { get; set; }
        public int Succeeded { get; set; }
        public int Failed { get; set; }
        public List<BulkUploadErrorDto> Errors { get; set; } = new();
    }

    public class BulkUploadErrorDto
    {
        public int Row { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
