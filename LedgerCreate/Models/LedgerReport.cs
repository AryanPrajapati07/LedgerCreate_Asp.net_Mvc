namespace LedgerCreate.Models
{
    public class LedgerReport
    {
        public DateTime TransactionDate { get; set; }
        public string Particulars { get; set; }
        public string VoucherNo { get; set; }
        public decimal DebitAmount { get; set; }
        public decimal CreditAmount { get; set; }
        public decimal Balance { get; set; }
    }

}
