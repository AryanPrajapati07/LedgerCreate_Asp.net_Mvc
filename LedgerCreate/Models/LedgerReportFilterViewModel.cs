namespace LedgerCreate.Models
{
    public class LedgerReportFilterViewModel
    {
        public string LedgerName { get; set; }
        public string LedgerGroup { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public List<LedgerReport> ReportList { get; set; }
        public List<string> AvailableGroups { get; set; }  // for dropdown
    }

}
