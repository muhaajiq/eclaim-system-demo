using MHA.ECLAIM.Entities.Entities;
using MHA.ECLAIM.Entities.ViewModel.Workflow;

namespace MHA.ECLAIM.Entities.ViewModel.Claim
{
    public class ApprovalFormVM
    {
        public int? RequestID { get; set; }
        public int? ProcessID { get; set; }
        public int? TaskID { get; set; }
        public MainClaimHeaderVM MainClaimHeaderVM { get; set; } = new MainClaimHeaderVM();
        public List<SubClaimDetailsVM> SubClaimDetailsVM { get; set; } = new List<SubClaimDetailsVM>();

        public PartialModelWorkflowHistory WFHistory { get; set; } = new PartialModelWorkflowHistory();

        public PartialModelExpenseModal ExpenseModal { get; set; } = new PartialModelExpenseModal();

        //Permission checking
        public string CurrentUser { get; set; } = string.Empty;
        public List<string> AccessGroups { get; set; } = new();
        public List<int> AccessDepartments { get; set; } = new();
        public bool IsMyTask { get; set; } = false;
        public string CurrentStage { get; set; } = string.Empty;
        public bool IsValid { get; set; } = false;
        public bool IsLoaded { get; set; } = false;

        // Error Logging
        public bool HasSuccess { get; set; } = false;
        public bool HasError { get; set; } = false;
        public string ErrorMessage { get; set; } = string.Empty;

        public string ModifiedBy { get; set; } = string.Empty;
        public string ModifiedByLogin { get; set; } = string.Empty;
        public DateTime? ModifiedByDate { get; set; }
    }
}
