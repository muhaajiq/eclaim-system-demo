using MHA.ECLAIM.Entities.ViewModel.Shared;
using Microsoft.AspNetCore.Components.Forms;

namespace MHA.ECLAIM.Entities.ViewModel.Claim
{
    public class SubClaimDetailsVM
    {
        #region SubClaimDetailsDbTable
        public int ID { get; set; }
        public int MainClaimHeaderID { get; set; }
        public DateTime? TransactionDate { get; set; }
        public string ClaimCategory { get; set; } = string.Empty;
        public string Expenses { get; set; } = string.Empty;
        public string ExpensesSubtype { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal? Quantity { get; set; }
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
        #endregion

        public bool FixedQuantity { get; set; }
        public int? FixedQuantityValue { get; set; }
        public bool FixedAmount { get; set; }
        public decimal? FixedAmountValue { get; set; }

        public int? ClaimEntitlementTypeSpId { get; set; }
        public byte[]? AttachmentFileBytes { get; set; }
        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }
        public decimal? ClaimLimit { get; set; }
        public string Frequency { get; set; } = string.Empty;
        public List<PeoplePickerUser> ClaimItemApprover { get; set; } = new();

        public Guid TempId { get; set; } = Guid.NewGuid();

        public ClaimFrequency? ClaimFrequencyEnum
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Frequency)) return null;
                return Frequency.ToLower() switch
                {
                    "per receipt" => ClaimFrequency.PerReceipt,
                    "daily" => ClaimFrequency.Daily,
                    "monthly" => ClaimFrequency.Monthly,
                    "lifetime" => ClaimFrequency.Lifetime,
                    _ => null
                };
            }
        }
    }

    public enum ClaimFrequency
    {
        None = 0,
        PerReceipt,
        Daily,
        Monthly,
        Lifetime
    }
}