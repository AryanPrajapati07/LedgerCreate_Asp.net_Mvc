using LedgerCreate.Models;

namespace LedgerCreate.Models
{
    public class LedgerReportWithChartViewModel
    {
        public List<LedgerReport> Reports { get; set; }
        public string ChartImageBase64 { get; set; }
    }
}
