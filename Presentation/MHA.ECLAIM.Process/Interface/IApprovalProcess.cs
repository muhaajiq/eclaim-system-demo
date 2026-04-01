using MHA.ECLAIM.Entities.Entities;
using MHA.ECLAIM.Entities.ViewModel.Claim;
using MHA.ECLAIM.Entities.ViewModel.Shared;

namespace MHA.ECLAIM.Process.Interface
{
    public interface IApprovalProcess
    {
        Task<ApprovalFormVM> InitApprovalForm(ApprovalFormVM vm, string spHostUrl, string accessToken);
        Task<RequestFormVisibilitySettings> SetVisibilitySettings(string currentStage);
        Task<PartialModelExpenseModal> GetExpenseModalInfo(SubClaimDetailsVM vm, string spHostUrl, string accessToken);
        Task<ApprovalFormVM> RequireAmendmentRequest(ApprovalFormVM vm, string spHostUrl, string accessToken);
        Task<ApprovalFormVM> ApproveRequest(ApprovalFormVM vm, string spHostUrl, string accessToken);
        Task<ApprovalFormVM> RejectRequest(ApprovalFormVM vm, string spHostUrl, string accessToken);
        Task<ApprovalFormVM> SaveRequest(ApprovalFormVM vm, string spHostUrl, string accessToken);
        Task<ApprovalFormVM> ReSubmitClaimRequest(ApprovalFormVM vm, string spHostUrl, string accessToken);
    }
}
