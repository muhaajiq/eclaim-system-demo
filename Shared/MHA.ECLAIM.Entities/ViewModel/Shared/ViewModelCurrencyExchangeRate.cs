namespace MHA.ECLAIM.Entities.ViewModel.Shared
{
    public class ViewModelCurrencyExchangeRate
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string BaseCurrency { get; set; } = string.Empty;
        public string TargetCurrency { get; set; } = string.Empty;
        public decimal Rate { get; set; }
    }
}
