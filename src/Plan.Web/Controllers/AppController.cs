using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Plan.Services;

namespace Plan.Web.Controllers
{
    [ApiController]
    [Route(PlanConst.ApiPrefx + "[controller]")]
    public class AppController : ControllerBase
    {
        public static readonly string LoginUri = PlanConst.ApiPrefx + typeof(AppController).Name.Replace("Controller", string.Empty) + "/" + nameof(Login);
        private readonly AppService appService;

        public AppController(AppService appService)
        {
            this.appService = appService;
        }

        [HttpGet("[action]")]
        [Authorize(AuthenticationSchemes = PlanAuthenticationHandler.SchemeName)]
        public async Task<IActionResult> Create([FromQuery] long userId, [FromQuery] DateTime? endTime)
        {
            if (userId <= 0)
            {
                return BadRequest(nameof(userId));
            }
            var res = await appService.CreateAppAsync(userId, endTime);
            return Ok(res);
        }
        [AllowAnonymous]
        [HttpPost("[action]")]
        public async Task<IActionResult> Login([FromQuery] string appKey, [FromQuery] long timestamp, [FromQuery] string sign)
        {
            if (string.IsNullOrEmpty(appKey))
            {
                return BadRequest(nameof(appKey));
            }
            if (timestamp<=0)
            {
                return BadRequest(nameof(timestamp));
            }
            if (string.IsNullOrEmpty(sign))
            {
                return BadRequest(nameof(sign));
            }
            var res = await appService.LoginAsync(appKey, timestamp, sign);
            return Ok(res);
        }
    }
}
