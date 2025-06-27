namespace LedgerCreate.Models
{
    public class LedgerReportAuditLog
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string LedgerName { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public DateTime GeneratedAt { get; set; }
        public string Status { get; set; }
        public string FileName { get; set; }
        public string IPAddress { get; set; }
    }

}
