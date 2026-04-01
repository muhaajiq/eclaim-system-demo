namespace MHA.ECLAIM.Entities.ViewModel.Home
{
    public class ViewModelAdministrationListing
    {
        public bool IsAuthorized { get; set; }

        //Project Settings
        public string ReportURL { get; set; }

        //Running Number Settings
        public string RunningNumberFormatURL { get; set; }
        public string RunningNumberURL { get; set; }

        //Master Data List
        public string EmployeeURL { get; set; }
        public string CountriesURL { get; set; }
        public string CostCenterURL { get; set; }
        public string DepartmentURL { get; set; }
        public string CompanyURL { get; set; }
        public string PositionURL { get; set; }

        public string ClaimEntitlementTypeURL { get; set; }
        public string ClaimItemApproverURL { get; set; }
        public string ClaimMasterApproverURL { get; set; }

        public string CurrencyURL { get; set; }
        public string TaxCodeURL { get; set; }
        public string GLCodeURL { get; set; }

        public string CategoryURL { get; set; }
        public string SubCategoryURL { get; set; }
        public string FrequencyURL { get; set; }

        public ViewModelAdministrationListing()
        {
            IsAuthorized = false;
        }
    }
}
