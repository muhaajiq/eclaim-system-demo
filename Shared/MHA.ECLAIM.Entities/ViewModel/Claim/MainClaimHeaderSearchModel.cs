namespace MHA.ECLAIM.Entities.ViewModel.Claim
{
    public class MainClaimHeaderSearchModel
    {
        public string RequesterCompanyCode { get; set; } = string.Empty;
        public string RequesterEmployeeID { get; set; } = string.Empty;
        public string RequesterName { get; set; } = string.Empty;
        public List<string> RequesterDepartment { get; set; } = new();
        public string ReferenceNo { get; set; } = string.Empty;
        public DateTime? RequestStartDate { get; set; }
        public DateTime? RequestEndDate { get; set; }
        public List<string> RequesterCostCenter { get; set; } = new();
        public List<string> ClaimStatus { get; set; } = new();

        //Permission Checking
        public string CurrentUser { get; set; } = string.Empty;
        public string CurrentUserLogin { get; set; } = string.Empty;
        public List<string> AccessGroups { get; set; } = new();
        public List<int> AccessDepartments { get; set; } = new();
        public string MemberLogin { get; set; } = string.Empty;

        public Byte[]? ExcelFileBytes { get; set; }
        public bool IsExportToExcel { get; set; }
    }
}
