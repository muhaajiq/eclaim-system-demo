using MHA.ECLAIM.Business;
using MHA.ECLAIM.Entities.DTO;
using MHA.ECLAIM.Entities.ViewModel.Claim;
using MHA.ECLAIM.Entities.ViewModel.Shared;
using MHA.ECLAIM.Framework.Constants;
using MHA.ECLAIM.Framework.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace MHA.ECLAIM.WebAPI.Controllers
{
    [ApiController]
    public class ClaimController : ControllerBase
    {
        private readonly SearchBL _searchBL;
        private readonly ClaimRequestBL _claimRequestBL;

        public ClaimController(SearchBL searchBL, ClaimRequestBL claimRequestBL)
        {
            _searchBL = searchBL;
            _claimRequestBL = claimRequestBL;
        }

        #region Init
        [HttpPost]
        [Route(ConstantHelper.API.ClaimPath.InitClaim)]
        public async Task<IActionResult> InitClaim([FromBody] APIRequestDTO<MainClaimHeaderVM> request)
        {
            try
            {
                var result = await _claimRequestBL.InitClaim(request.Data, request.SpHostUrl, request.AccessToken);
                return Ok(result);
            }
            catch (Exception ex)
            {
                LogHelper logHelper = new LogHelper();
                logHelper.LogMessage($"[API] ClaimController.InitClaim Error: {ex}");
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Route(ConstantHelper.API.ClaimPath.InitClaimDisplayForm)]
        public async Task<IActionResult> InitClaimDisplayForm([FromBody] APIRequestDTO<int> request)
        {
            try
            {
                var result = await _claimRequestBL.InitClaimDisplayForm(request.Data, request.SpHostUrl, request.AccessToken);
                return Ok(result);
            }
            catch (Exception ex)
            {
                LogHelper logHelper = new LogHelper();
                logHelper.LogMessage($"[API] ClaimController.InitClaimDisplayForm Error: {ex}");
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Route(ConstantHelper.API.ClaimPath.InitExpensesList)]
        public async Task<IActionResult> InitExpensesList([FromBody] APIRequestDTO<object> request)
        {
            try
            {
                var result = await _claimRequestBL.InitExpensesList(request.SpHostUrl, request.AccessToken);
                return Ok(result);
            }
            catch (Exception ex)
            {
                LogHelper logHelper = new LogHelper();
                logHelper.LogMessage($"[API] ClaimController.InitExpensesList Error: {ex}");
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region Save
        [HttpPost]
        [Route(ConstantHelper.API.ClaimPath.SaveClaimDetails)]
        public async Task<IActionResult> SaveClaimDetails([FromBody] APIRequestDTO<MainClaimHeaderVM> request)
        {
            try
            {
                var result = await _claimRequestBL.SaveClaimDetails(request.Data, request.SpHostUrl, request.AccessToken);
                return Ok(result);
            }
            catch (Exception ex)
            {
                LogHelper logHelper = new LogHelper();
                logHelper.LogMessage($"[API] ClaimController.SaveClaimDetails Error: {ex}");
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Route(ConstantHelper.API.ClaimPath.SaveNewRequestForm)]
        public async Task<IActionResult> SaveNewRequestForm([FromBody] APIRequestDTO<MainClaimHeaderVM> request)
        {
            try
            {
                var result = await _claimRequestBL.SaveNewRequestForm(request.Data, request.SpHostUrl, request.AccessToken);
                return Ok(result);
            }
            catch (Exception ex)
            {
                LogHelper logHelper = new LogHelper();
                logHelper.LogMessage($"[API] ClaimController.SaveNewRequestForm Error: {ex}");
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region Submit
        [HttpPost]
        [Route(ConstantHelper.API.ClaimPath.SubmitClaimRequestForm)]
        public async Task<IActionResult> SubmitClaimRequestForm([FromBody] APIRequestDTO<MainClaimHeaderVM> request)
        {
            try
            {
                var result = await _claimRequestBL.SubmitClaimRequestForm(request.Data, request.SpHostUrl, request.AccessToken);
                return Ok(result);
            }
            catch (Exception ex)
            {
                LogHelper logHelper = new LogHelper();
                logHelper.LogMessage($"[API] ClaimController.SubmitClaimRequestForm Error: {ex}");
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region Search
        [HttpPost]
        [Route(ConstantHelper.API.ClaimPath.SearchClaim)]
        public async Task<IActionResult> GetPagedSearch([FromBody] APIRequestDTO<PagedRequest<MainClaimHeaderSearchModel>> request)
        {
            var result = await _searchBL.GetPagedClaims(request.Data.Search, request.Data.SortColumn, request.Data.SortDirection, request.Data.Skip, request.Data.Take, request.SpHostUrl, request.AccessToken);
            return Ok(result);
        }
        #endregion

        #region Delete
        [HttpPost]
        [Route(ConstantHelper.API.ClaimPath.DeleteClaimAttachment)]
        public async Task<IActionResult> DeleteClaimAttachment([FromBody] APIRequestDTO<MainClaimHeaderVM> request)
        {
            try
            {
                var result = await _claimRequestBL.DeleteClaimAttachment(request.Data, request.SpHostUrl, request.AccessToken);
                return Ok(result);
            }
            catch (Exception ex)
            {
                LogHelper logHelper = new LogHelper();
                logHelper.LogMessage($"[API] ClaimController.DeleteClaimAttachment Error: {ex}");
                return BadRequest(ex.Message);
            }
        }
        
        [HttpPost]
        [Route(ConstantHelper.API.ClaimPath.DeleteClaimDetails)]
        public async Task<IActionResult> DeleteClaimDetails([FromBody] APIRequestDTO<MainClaimHeaderVM> request)
        {
            try
            {
                var result = await _claimRequestBL.DeleteClaimDetails(request.Data, request.SpHostUrl, request.AccessToken);
                return Ok(result);
            }
            catch (Exception ex)
            {
                LogHelper logHelper = new LogHelper();
                logHelper.LogMessage($"[API] ClaimController.DeleteClaimDetails Error: {ex}");
                return BadRequest(ex.Message);
            }
        }
        #endregion
    }
}
