using MHA.ECLAIM.Entities.ViewModel.Shared;

namespace MHA.ECLAIM.Entities.ViewModel.Claim
{
    public class MainClaimHeaderVM
    {
        #region MainClaimHeaderDbTable
        public int? ID { get; set; }
        public string RequesterCompanyCode { get; set; } = string.Empty;
        public string RequesterCompanyName { get; set; } = string.Empty;
        public string RequesterEmployeeID { get; set; } = string.Empty;
        public string RequesterDepartment { get; set; } = string.Empty;
        public string RequesterName { get; set; } = string.Empty;
        public string RequesterLogin { get; set; } = string.Empty;
        public string RequesterCostCenter { get; set; } = string.Empty;

        public DateTime? RequestDate { get; set; }
        public string ReferenceNo { get; set; } = string.Empty;
        public string ClaimStatus { get; set; } = string.Empty;
        public string PurposeOfClaims { get; set; } = string.Empty;
        public string ClaimCurrency { get; set; } = string.Empty;
        public string TravelRequestFormRef { get; set; } = string.Empty;
        public string SelectedTravelRequestId { get; set; } = string.Empty;
        public string ClaimCategory { get; set; } = string.Empty;
        public string SelectedClaimCategorySpId { get; set; } = string.Empty;

        public string ClaimEntitlementType1 { get; set; } = string.Empty;
        public int? ClaimEntitlementType1SpId { get; set; }
        public DateTime? ClaimEntitlementType1StartDate { get; set; }
        public DateTime? ClaimEntitlementType1EndDate { get; set; }
        public string ClaimEntitlementType2 { get; set; } = string.Empty;
        public int? ClaimEntitlementType2SpId { get; set; }
        public DateTime? ClaimEntitlementType2StartDate { get; set; }
        public DateTime? ClaimEntitlementType2EndDate { get; set; }
        public string CurrentClaimEntitlementType { get; set; } = string.Empty;
        public int? CurrentClaimEntitlementTypeSpId { get; set; }
        public string GeneralRemarks { get; set; } = string.Empty;

        public string CreatedBy { get; set; } = string.Empty;
        public string CreatedByLogin { get; set; } = string.Empty;
        public DateTime? CreatedDate { get; set; }

        public string ModifiedBy { get; set; } = string.Empty;
        public string ModifiedByLogin { get; set; } = string.Empty;
        public DateTime? ModifiedDate { get; set; }

        public string SubmittedBy { get; set; } = string.Empty;
        public string SubmittedByLogin { get; set; } = string.Empty;
        public DateTime? SubmittedDate { get; set; }
        public string? SubmittedByEmail { get; set; }

        public decimal? TotalClaimAmount { get; set; }
        public string TempFolderGuid { get; set; } = string.Empty;

        public int? DepartmentID { get; set; }

        #endregion

        #region SubClaimDetailsList
        public List<SubClaimDetailsVM> InitSubClaimDetailsVM { get; set; } = new();
        public List<SubClaimDetailsVM> UserAddedSubClaimDetailsListing { get; set; } = new();
        public List<SubClaimDetailsVM> SubClaimDetails { get; set; } = new();
        public SubClaimDetailsVM CurrentClaimDetails { get; set; } = new();
        public List<SubClaimDetailsVM> ExpensesMaintenanceList { get; set; } = new();

        #endregion

        #region Dropdown List
        public List<DropDownListItem> ClaimCategories { get; set; } = new();
        public List<DropDownListItem> CurrencyList { get; set; } = new();
        public List<DropDownListItem> TravelRequestRefNoList { get; set; } = new();
        public List<DropDownListItem> ExpensesList { get; set; } = new();
        public List<DropDownListItem> ExpensesSubtypeList { get; set; } = new();
        #endregion

        #region Currency Exchange
        public List<ViewModelCurrencyExchangeRate> CurrencyExchangeRates { get; set; } = new();
        #endregion

        //#region Searching
        //public MainClaimHeaderSearchModel SearchModel = new();
        //public List<MainClaimHeaderEntity> MainClaimHeaderListing = new();
        //public int TotalCount { get; set; }
        //public bool ShowInactiveItems { get; set; } = false;
        //#endregion

        //#region Approval Action
        //public bool ShowApproveButton { get; set; }
        //public bool ShowRejectButton { get; set; }
        //public bool ShowCompleteButton { get; set; }
        //public bool ShowRequireAmendment { get; set; }
        //public bool ShowResubmitButton { get; set; }
        //#endregion

        #region Workflow
        public string WorkflowStage { get; set; } = string.Empty;
        public bool IsMyTask { get; set; }
        public int RequestID { get; set; }
        public int ProcessID { get; set; }
        public int TaskID { get; set; }
        #endregion

        #region SP Group Permission Settings
        public HashSet<string> AccessGroup { get; set; } = new();
        #endregion

        #region Common VM Properties

        public bool HasError { get; set; }

        public string ErrorMessage { get; set; } = string.Empty;

        public bool IsSuccessful { get; set; }

        public string SuccessMessage { get; set; } = string.Empty;

        public bool HasActionError { get; set; }

        public string ActionErrorMessage { get; set; } = string.Empty;

        #endregion
    }
}
