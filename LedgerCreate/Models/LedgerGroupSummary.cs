using LedgerCreate.Models;
namespace LedgerCreate.Models
{
    public class LedgerGroupSummary
    {
        public string LedgerGroup { get; set; }
        public decimal TotalDebit { get; set; }
        public decimal TotalCredit { get; set; }
        public decimal Balance { get; set; }
    }

}
