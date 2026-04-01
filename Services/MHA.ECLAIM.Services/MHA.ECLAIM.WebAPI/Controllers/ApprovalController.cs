using MHA.ECLAIM.Business;
using MHA.ECLAIM.Entities.DTO;
using MHA.ECLAIM.Entities.Entities;
using MHA.ECLAIM.Entities.ViewModel.Claim;
using MHA.ECLAIM.Framework.Constants;
using Microsoft.AspNetCore.Mvc;

namespace MHA.ECLAIM.WebAPI.Controllers
{
    [ApiController]
    public class ApprovalController : ControllerBase
    {
        private readonly ILogger<ClaimController> _logger;
        private readonly ApprovalFormBL _approvalFormBL;

        public ApprovalController(ILogger<ClaimController> logger, ApprovalFormBL approvalFormBL)
        {
            _logger = logger;
            _approvalFormBL = approvalFormBL;
        }

        [HttpPost]
        [Route(ConstantHelper.API.ClaimPath.InitApprovalForm)]
        public async Task<IActionResult> InitApprovalForm([FromBody] APIRequestDTO<ApprovalFormVM> request)
        {
            try
            {
                var result = await _approvalFormBL.InitApprovalForm(request.Data, request.SpHostUrl, request.AccessToken);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Route(ConstantHelper.API.ClaimPath.SetVisibilitySettings)]
        public async Task<IActionResult> SetVisibilitySettings([FromBody] APIRequestDTO<string> request)
        {
            try
            {
                var result = await _approvalFormBL.SetVisibilitySettings(request.Data);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Route(ConstantHelper.API.ClaimPath.GetExpenseModalInfo)]
        public async Task<IActionResult> GetExpenseModalInfo([FromBody] APIRequestDTO<SubClaimDetails> request)
        {
            try
            {
                var result = await _approvalFormBL.GetExpenseModalInfo(request.Data, request.SpHostUrl, request.AccessToken);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Route(ConstantHelper.API.ClaimPath.ApproveRequest)]
        public async Task<IActionResult> ApproveRequest([FromBody] APIRequestDTO<ApprovalFormVM> request)
        {
            try
            {
                var result = await _approvalFormBL.ApproveRequest(request.Data, request.SpHostUrl, request.AccessToken);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Route(ConstantHelper.API.ClaimPath.RequireAmendmentRequest)]
        public async Task<IActionResult> RequireAmendmentRequest([FromBody] APIRequestDTO<ApprovalFormVM> request)
        {
            try
            {
                var result = await _approvalFormBL.RequireAmendmentRequest(request.Data, request.SpHostUrl, request.AccessToken);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Route(ConstantHelper.API.ClaimPath.RejectRequest)]
        public async Task<IActionResult> RejectRequest([FromBody] APIRequestDTO<ApprovalFormVM> request)
        {
            try
            {
                var result = await _approvalFormBL.RejectRequest(request.Data, request.SpHostUrl, request.AccessToken);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Route(ConstantHelper.API.ClaimPath.SaveRequest)]
        public async Task<IActionResult> SaveRequest([FromBody] APIRequestDTO<ApprovalFormVM> request)
        {
            try
            {
                var result = await _approvalFormBL.SaveRequest(request.Data, request.SpHostUrl, request.AccessToken);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Route(ConstantHelper.API.ClaimPath.ReSubmitClaimRequest)]
        public async Task<IActionResult> ReSubmitClaimRequest([FromBody] APIRequestDTO<ApprovalFormVM> request)
        {
            try
            {
                var result = await _approvalFormBL.ReSubmitClaimRequest(request.Data, request.SpHostUrl, request.AccessToken);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
