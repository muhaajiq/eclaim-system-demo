using MHA.ECLAIM.Entities.ViewModel.Home;

namespace MHA.ECLAIM.Process.Interface
{
    public interface IAdministrationProcess
    {
        Task<ViewModelAdministrationListing> InitAdministrationListing(string spHostUrl, string accessToken);
    }
}
