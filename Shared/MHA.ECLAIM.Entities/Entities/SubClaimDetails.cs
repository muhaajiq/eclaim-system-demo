using System.ComponentModel.DataAnnotations.Schema;

namespace MHA.ECLAIM.Entities.Entities
{
    [Table("ecSubClaimDetails")]
    public class SubClaimDetails
    {
        public int ID { get; set; }
        public int MainClaimHeaderID { get; set; }
        public DateTime? TransactionDate { get; set; }
        public string ClaimCategory { get; set; } = string.Empty;
        public string Expenses { get; set; } = string.Empty;
        public string ExpensesSubtype { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int? Quantity { get; set; }
        public string TransactionCurrency { get; set; } = string.Empty;
        public decimal? UnitPrice { get; set; }
        public decimal? TransactionAmount { get; set; }
        public string ClaimCurrency { get; set; } = string.Empty;
        public decimal? UnitPriceConverted { get; set; }
        public decimal? TransactionAmountConverted { get; set; }
        public decimal? SubtotalClaimAmount { get; set; }
        public decimal? TotalClaimAmount { get; set; }
        public string? AttachmentPathUrl { get; set; }
        public string? AttachmentFileName { get; set; }
        public decimal? ExchangeRate { get; set; }
        public string ClaimEntitlementType { get; set; } = string.Empty;
        public string TaxCode { get; set; } = string.Empty;
        public bool IsSelected { get; set; }
        public string? CreatedBy { get; set; }
        public string? CreatedByLogin { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? ModifiedBy { get; set; }
        public string? ModifiedByLogin { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }
}
