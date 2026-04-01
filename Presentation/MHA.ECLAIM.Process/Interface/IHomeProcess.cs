using MHA.ECLAIM.Entities.ViewModel.Home;

namespace MHA.ECLAIM.Process.Interface
{
    public interface IHomeProcess
    {
        Task<ViewModelMyPendingTask> GetMyPendingTask(ViewModelMyPendingTask vmMyTask, string spHostUrl, string accessToken);

        Task<ViewModelMyActiveRequest> GetMyActiveRequest(ViewModelMyActiveRequest vmMyTask, string spHostUrl, string accessToken);
    }
}
