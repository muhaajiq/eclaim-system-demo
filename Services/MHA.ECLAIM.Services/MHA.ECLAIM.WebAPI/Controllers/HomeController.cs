using MHA.ECLAIM.Business;
using MHA.ECLAIM.Entities.DTO;
using MHA.ECLAIM.Entities.ViewModel.Home;
using MHA.ECLAIM.Entities.ViewModel.Shared;
using MHA.ECLAIM.Framework.Constants;
using Microsoft.AspNetCore.Mvc;

namespace MHA.ECLAIM.WebAPI.Controllers
{
    [ApiController]
    public class HomeController : ControllerBase
    {
        private readonly ILogger<HomeController> _logger;
        private readonly HomeBL _homeBL;
        public HomeController(ILogger<HomeController> logger, HomeBL homeBL)
        {
            _logger = logger;
            _homeBL = homeBL;
        }

        [HttpPost]
        [Route(ConstantHelper.API.HomePath.GetMyPendingTask)]
        public async Task<IActionResult> GetMyPendingTask([FromBody] APIRequestDTO<ViewModelMyPendingTask> request)
        {
            try
            {
                ViewModelMyPendingTask result = await _homeBL.GetMyPendingTask(request.Data, request.SpHostUrl, request.AccessToken);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Route(ConstantHelper.API.HomePath.GetMyActiveRequest)]
        public async Task<IActionResult> GetMyActiveRequest([FromBody] APIRequestDTO<ViewModelMyActiveRequest> request)
        {
            try
            {
                ViewModelMyActiveRequest result = await _homeBL.GetMyActiveRequest(request.Data, request.SpHostUrl, request.AccessToken);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
