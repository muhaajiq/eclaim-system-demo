using MHA.ECLAIM.Entities.Entities;
using MHA.ECLAIM.Entities.ViewModel.Workflow;

namespace MHA.ECLAIM.Entities.ViewModel.Claim
{
    public class ViewClaimVM
    {
        public int RequestId { get; set; }
        public MainClaimHeaderVM MainClaimHeaderVM { get; set; } = new MainClaimHeaderVM();
        public List<SubClaimDetailsVM> SubClaimDetailsVM { get; set; } = new List<SubClaimDetailsVM>();

        public PartialModelWorkflowHistory WFHistory = new PartialModelWorkflowHistory();

        public PartialModelAdminWorkflowHistory AdminWFHistory = new PartialModelAdminWorkflowHistory();
        public PartialModelExpenseModal ExpenseModal { get; set; } = new PartialModelExpenseModal();

        //Permission checking
        public string CurrentUser { get; set; } = string.Empty;
        public string MemberLogin { get; set; } = string.Empty;
        public List<string> AccessGroups { get; set; } = new();
        public List<int> AccessDepartments { get; set; } = new();
        public bool IsValid { get; set; } = false;
        public bool IsLoaded { get; set; } = false;

        // Error Logging
        public bool IsSuccessful { get; set; } = false;
        public bool HasError { get; set; } = false;
        public string ErrorMessage { get; set; } = string.Empty;
    }
}
