
using MHA.ECLAIM.Entities.ViewModel.Shared;
using MHA.ECLAIM.Entities.ViewModel.Workflow;

namespace MHA.ECLAIM.Process.Interface
{
    public interface IWorkflowProcess
    {
        public void UpdateRequestWF(string refNo, List<string> paramNamesList, List<object> paramObjectsList);
        Task<PartialModelWorkflowHistory> InitWorkflowHistory(int processID, string spHostURL, string accessToken);
        Task<PartialModelAdminWorkflowHistory> InitAdminWFHistory(int processID, string spHostURL, string accessToken);
        Task<bool> AddActioner(AddActionerVM model, string spHostURL, string accessToken, string appAccessToken);
        Task<bool> ReassignActioner(ReassignActionerVM model, string spHostURL, string accessToken);
        Task<bool> RemoveActioner(RemoveActionerVM model, string spHostURL, string accessToken, string appAccessToken);
        Task<bool> RemoveAllActioner(RemoveAllActionerVM model, string spHostURL, string accessToken, string appAccessToken);
        public Task<string> SetDelegation(ViewModelNewDelegate vmNewDelegate, string spHostURL, string accessToken);
        public Task<ViewModelNewDelegate> InitNewDelegate(string spHostURL, string accessToken);
        public Task<ViewModelMyDelegate> InitMyDelegate(string spHostURL, string accessToken);
        public Task DeleteDelegation(int delegationID, string spHostURL, string accessToken);

        Task<ViewModelWorkflowReport> InitAuthWorkflowReportCheck(ViewModelWorkflowReport vm, string spHostURL, string accessToken);
        Task<ViewModelWorkflowReport> SearchWorkflowTaskReport(ViewModelWorkflowReport vm, string spHostURL, string accessToken);
        Task<ViewModelWorkflowReport> ExportToExcelWorkflowTaskReport(ViewModelWorkflowReport vm, string spHostURL, string accessToken);
        
    }
}
