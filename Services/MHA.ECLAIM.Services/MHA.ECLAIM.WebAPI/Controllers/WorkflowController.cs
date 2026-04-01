using MHA.ECLAIM.Business;
using MHA.ECLAIM.Entities.DTO;
using MHA.ECLAIM.Entities.ViewModel.Workflow;
using MHA.ECLAIM.Framework.Constants;
using MHA.ECLAIM.Entities;
using Microsoft.AspNetCore.Mvc;
namespace MHA.ECLAIM.WebAPI.Controllers
{
    [ApiController]
    public class WorkflowController : ControllerBase
    {
        private readonly ILogger<WorkflowController> _logger;

        public WorkflowController(ILogger<WorkflowController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        [Route(ConstantHelper.API.WorkflowPath.InitWorkflowHistory)]
        public async Task<IActionResult> InitWorkflowHistory([FromBody] APIRequestDTO<int> request)
        {
            try
            {
                WorkflowBL workflowBL = new WorkflowBL();
                var result = await workflowBL.InitWorkflowHistory(request.Data, request.SpHostUrl, request.AccessToken);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Route(ConstantHelper.API.WorkflowPath.InitAdminWFHistory)]
        public async Task<IActionResult> InitAdminWFHistory([FromBody] APIRequestDTO<int> request)
        {
            try
            {
                WorkflowBL workflowBL = new WorkflowBL();
                var result = await workflowBL.InitAdminWFHistory(request.Data, request.SpHostUrl, request.AccessToken);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Route(ConstantHelper.API.WorkflowPath.AddActioner)]
        public async Task<IActionResult> AddActioner([FromBody] APIRequestDTO<AddActionerVM> request)
        {
            try
            {
                WorkflowBL workflowBL = new WorkflowBL();
                var result = await workflowBL.AddActioner(request.Data.ProcessID, request.Data.NewActioner, request.SpHostUrl, request.AccessToken, request.AppAccessToken);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Route(ConstantHelper.API.WorkflowPath.ReassignActioner)]
        public async Task<IActionResult> ReassignActioner([FromBody] APIRequestDTO<ReassignActionerVM> request)
        {
            try
            {
                WorkflowBL workflowBL = new WorkflowBL();
                var result = await workflowBL.ReassignActioner(request.Data.ProcessID, request.Data.TaskID, request.Data.NewActioner, request.SpHostUrl, request.AccessToken, request.AppAccessToken);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Route(ConstantHelper.API.WorkflowPath.RemoveActioner)]
        public async Task<IActionResult> RemoveActioner([FromBody] APIRequestDTO<RemoveActionerVM> request)
        {
            try
            {
                WorkflowBL workflowBL = new WorkflowBL();
                var result = await workflowBL.RemoveActioner(request.Data.ProcessID, request.Data.TaskID, request.Data.WorkflowName, request.Data.Remarks, request.SpHostUrl, request.AccessToken, request.AppAccessToken);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Route(ConstantHelper.API.WorkflowPath.RemoveAllActioner)]
        public async Task<IActionResult> RemoveAllActioner([FromBody] APIRequestDTO<RemoveAllActionerVM> request)
        {
            try
            {
                WorkflowBL workflowBL = new WorkflowBL();
                var result = await workflowBL.RemoveAllActioner(request.Data.ProcessID, request.Data.TaskIDs, request.Data.WorkflowNames, request.Data.Remark, request.SpHostUrl, request.AccessToken, request.AppAccessToken);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Route(ConstantHelper.API.WorkflowPath.InitMyDelegate)]
        public async Task<IActionResult> InitMyDelegate([FromBody] APIRequestDTO<object> request)
        {
            try
            {
                WorkflowBL workflowBL = new WorkflowBL();
                var result = workflowBL.InitMyDelegate(request.SpHostUrl, request.AccessToken);
                return Ok(result.Result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Route(ConstantHelper.API.WorkflowPath.SetDelegation)]
        public async Task<IActionResult> SetDelegation([FromBody] APIRequestDTO<ViewModelNewDelegate> request)
        {
            try
            {
                WorkflowBL workflowBL = new WorkflowBL();
                var result = workflowBL.SetDelegation(request.Data, request.SpHostUrl, request.AccessToken);
                return Ok(result.Result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Route(ConstantHelper.API.WorkflowPath.InitNewDelegate)]
        public async Task<IActionResult> InitNewDelegate([FromBody] APIRequestDTO<object> request)
        {
            try
            {
                WorkflowBL workflowBL = new WorkflowBL();
                var result = workflowBL.InitNewDelegate(request.SpHostUrl, request.AccessToken);
                return Ok(result.Result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Route(ConstantHelper.API.WorkflowPath.DeleteDelegation)]
        public async Task<IActionResult> DeleteDelegation([FromBody] APIRequestDTO<int> request)
        {
            try
            {
                WorkflowBL workflowBL = new WorkflowBL();
                workflowBL.DeleteDelegation(request.Data, request.SpHostUrl, request.AccessToken);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Route(ConstantHelper.API.WorkflowPath.InitAuthWorkflowReportCheck)]

        public async Task<IActionResult> InitAuthWorkflowReportCheck([FromBody] APIRequestDTO<ViewModelWorkflowReport> request)
        {
            try
            {
                WorkflowBL workflowBL = new WorkflowBL();
   
                var result = workflowBL.CheckAuthorityWorkflowStatus(request.Data, request.SpHostUrl, request.AccessToken);
                return Ok(result.Result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Route(ConstantHelper.API.WorkflowPath.SearchWorflowTaskReport)]

        public async Task<IActionResult> SearchWorflowTaskReport([FromBody] APIRequestDTO<ViewModelWorkflowReport> request)
        {
            try
            {
                WorkflowBL workflowBL = new WorkflowBL();
                var result = workflowBL.SearchWorkflowTaskReport(request.Data, request.SpHostUrl, request.AccessToken);
                return Ok(result.Result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Route(ConstantHelper.API.WorkflowPath.ExportToExcelWorkflowTaskReport)]

        public async Task<IActionResult> ExportToExcelWorkflowTaskReport([FromBody] APIRequestDTO<ViewModelWorkflowReport> request)
        {
            try
            {
                WorkflowBL workflowBL = new WorkflowBL();
                var result = workflowBL.ExportToExcelWorkflowTaskReport(request.Data, request.SpHostUrl, request.AccessToken);
                return Ok(result.Result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }
}
