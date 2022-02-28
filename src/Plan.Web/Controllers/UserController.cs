using Plan.ChannelModel.Helpers;
using Plan.ChannelModel.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Plan.Services;
using Quartz;

namespace Plan.Web.Controllers
{
    [ApiController]
    [Route(PlanConst.ApiPrefx + "[controller]")]
    public class TestController : ControllerBase
    {
        private readonly ISchedulerFactory schedulerFactory;

        public TestController(ISchedulerFactory schedulerFactory)
        {
            this.schedulerFactory = schedulerFactory;
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> Run()
        {
            var sch = await schedulerFactory.GetScheduler();
            var key = JobBuilder.Create<LogJob>().Build();
            
            var trigger = TriggerBuilder.Create().ForJob(key)
                .WithSimpleSchedule(x =>
                {
                    x.WithInterval(TimeSpan.FromSeconds(10)).RepeatForever();
                }).Build();
            await sch.ScheduleJob(key, trigger);
            return Ok();
        }
    }
    [ApiController]
    [Route(PlanConst.ApiPrefx + "[controller]")]
    public class UserController : ControllerBase
    {
        private readonly UserService userService;

        public UserController(UserService userService)
        {
            this.userService = userService;
        }
        [Authorize(AuthenticationSchemes = PlanAuthenticationHandler.SkipAuthSchemeName)]
        [HttpGet("[action]")]
        [ProducesResponseType(typeof(EntityResult<RSAKeyIdentity>),200)]
        public async Task<IActionResult> FlushKey()
        {
            var key = await userService.FlushRSAKey();
            var res = new EntityResult<RSAKeyIdentity> { Data = key };
            return Ok(res);
        }
        [Authorize(AuthenticationSchemes = PlanAuthenticationHandler.SkipAuthSchemeName)]
        [HttpPost("[action]")]
        [ProducesResponseType(typeof(EntityResult<string>), 200)]
        public async Task<IActionResult> Login([FromForm]string userName, [FromForm] string passwordHash,[FromForm] string connectId)
        {
            var tk = await userService.LoginAsync(connectId, userName, passwordHash);
            if (!string.IsNullOrEmpty(tk))
            {
                HttpContext.Response.Cookies.Append(PlanAuthenticationHandler.AuthHeader, tk, new Microsoft.AspNetCore.Http.CookieOptions
                {
                    MaxAge = UserIdentityService.ExpireTime
                });
            }
            var res = new EntityResult<string> { Data = tk };
            return Ok(res);
        }
        [Authorize(AuthenticationSchemes = PlanAuthenticationHandler.SkipAuthSchemeName)]
        [HttpPost("[action]")]
        [ProducesResponseType(typeof(EntityResult<bool>), 200)]
        public async Task<IActionResult> Registe([FromForm] string userName, [FromForm] string passwordHash, [FromForm] string connectId)
        {
            var succeed = await userService.RegisteAsync(connectId, userName, passwordHash);
            var res = new EntityResult<bool> { Data = succeed };
            return Ok(res);
        }
        [Authorize(AuthenticationSchemes = PlanAuthenticationHandler.SkipAuthSchemeName)]
        [HttpGet("[action]")]
        [ProducesResponseType(typeof(EntityResult<bool>), 200)]
        public async Task<IActionResult> ResetPwd(string userName, string tk, string pwd)
        {
            var resetRes = await userService.RestPasswordAsync(HttpContext.Session.Id, userName, tk, pwd);
            var res = new EntityResult<bool> { Data = resetRes };
            return Ok(res);
        }
        [Authorize(AuthenticationSchemes = PlanAuthenticationHandler.SkipAuthSchemeName)]
        [HttpGet("[action]")]
        [ProducesResponseType(typeof(EntityResult<bool>), 200)]
        public async Task<IActionResult> ResetPwdWithOld(string userName, string old, string pwd)
        {
            var resetRes = await userService.RestPasswordWithOldAsync(HttpContext.Session.Id, userName, old, pwd);
            var res = new EntityResult<bool> { Data = resetRes };
            return Ok(res);
        }
        [Authorize(AuthenticationSchemes = PlanAuthenticationHandler.SkipAuthSchemeName)]
        [HttpGet("[action]")]
        [ProducesResponseType(typeof(EntityResult<string>), 200)]
        public async Task<IActionResult> GenerateResetToken(string userName)
        {
            //TODO:发邮件
            var tk = await userService.GenerateResetTokenAsync(userName);
            var res = new EntityResult<string> { Data = tk };
            return Ok(res);
        }
    }
}
