namespace MHA.ECLAIM.Entities.ViewModel.Claim
{
    public class PartialModelExpenseModal
    {
        public string Frequency { get; set; } = string.Empty;
        public decimal? ClaimLimit { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}
