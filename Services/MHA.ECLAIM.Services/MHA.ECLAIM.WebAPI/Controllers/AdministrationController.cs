using MHA.ECLAIM.Business;
using MHA.ECLAIM.Entities.DTO;
using MHA.ECLAIM.Framework.Constants;
using Microsoft.AspNetCore.Mvc;

namespace MHA.ECLAIM.WebAPI.Controllers
{
    [ApiController]
    public class AdministrationController : ControllerBase
    {
        private readonly ILogger<AdministrationController> _logger;

        public AdministrationController(ILogger<AdministrationController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        [Route(ConstantHelper.API.AdministrationPath.InitAdministrationListing)]
        public async Task<IActionResult> InitAdministrationListing([FromBody] APIRequestDTO<object> request)
        {
            try
            {
                AdministrationBL adminitrationBL = new AdministrationBL();
                var result = adminitrationBL.InitAdministrationListing(
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
