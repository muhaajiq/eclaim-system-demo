using MHA.ECLAIM.Business;
using MHA.ECLAIM.Entities.DTO;
using MHA.ECLAIM.Entities.ViewModel.Report;
using MHA.ECLAIM.Framework.Constants;
using Microsoft.AspNetCore.Mvc;

namespace MHA.ECLAIM.WebAPI.Controllers
{
    [ApiController]
    public class ReportController : ControllerBase
    {
        private readonly ILogger<ReportController> _logger;

        public ReportController(ILogger<ReportController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        [Route(ConstantHelper.API.ReportPath.GetReportListing)]
        public async Task<IActionResult> GetReportListing([FromBody] APIRequestDTO<object> request)
        {
            try
            {
                ReportBL reportBL = new ReportBL();
                var result = reportBL.InitReportListing(
                    //request.Payload,
                    request.SpHostUrl,
                    request.AccessToken);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
