using MHA.ECLAIM.Entities.Entities;
using MHA.ECLAIM.Entities.ViewModel.Claim;
using MHA.ECLAIM.Entities.ViewModel.Search;
using MHA.ECLAIM.Entities.ViewModel.Shared;

namespace MHA.ECLAIM.Process.Interface
{
    public interface IClaimProcess
    {
        #region Init
        Task<MainClaimHeaderVM> InitClaim(MainClaimHeaderVM vm, string spHostUrl, string accessToken);
        Task<MainClaimHeaderVM> InitExpensesList(string spHostUrl, string accessToken);
        Task<ViewClaimVM> InitClaimDisplayForm(int requestId, string spHostUrl, string accessToken);
        #endregion

        #region Save
        Task<MainClaimHeaderVM> SaveClaimDetails(MainClaimHeaderVM vm, string spHostUrl, string accessToken);
        Task<MainClaimHeaderVM> SaveNewRequestForm(MainClaimHeaderVM vm, string spHostUrl, string accessToken);
        #endregion

        #region Submit
        Task<MainClaimHeaderVM> SubmitClaimRequestForm(MainClaimHeaderVM vm, string spHostUrl, string accessToken);
        #endregion

        #region Search
        Task<SearchClaimVM> SearchClaims(MainClaimHeaderSearchModel search, string sortCol, string sortDirection, int skip, int take, string spHostUrl, string accessToken);
        #endregion

        #region Delete
        Task<bool> DeleteClaimAttachment(MainClaimHeaderVM vm, string spHostUrl, string accessToken);
        Task<bool> DeleteClaimDetails(MainClaimHeaderVM vm, string spHostUrl, string accessToken);
        #endregion
    }
}
