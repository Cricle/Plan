using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Plan.Services;
using Plan.Web.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Plan.Web
{
    internal class PlanAuthenticationHandler : IAuthenticationHandler
    {
        public const string SkipAuthSchemeName = "sktip-auth";

        public const string SchemeName = "plan";
        public const string AuthHeader = "plan-auth-token";
        public const string AccessHeader = "plan-access-token";
        public const string APPKeyHeader = "plan-app-key";

        private readonly UserIdentityService userIdentityService;
        private readonly AppService appService;

        private HttpContext context;
        private bool isApiQuery;
        private bool isAppLogin;

        public PlanAuthenticationHandler(UserIdentityService userIdentityService,
            AppService appService)
        {
            this.appService = appService;
            this.userIdentityService = userIdentityService;
        }
        private string GetFromHeaderOrCookie(string key)
        {
            var accessToken = context.Request.Headers[AccessHeader];
            var accessTk = accessToken.ToString();
            if (string.IsNullOrEmpty(accessTk))
            {
                context.Request.Cookies.TryGetValue(AuthHeader, out accessTk);
            }
            return accessTk;
        }
        public async Task<AuthenticateResult> AuthenticateAsync()
        {
            if (!isAppLogin)
            {
                var accessToken = GetFromHeaderOrCookie(AccessHeader);
                var appKey = GetFromHeaderOrCookie(AccessHeader);
                if (string.IsNullOrEmpty(accessToken)|| string.IsNullOrEmpty(appKey))
                {
                    return AuthenticateResult.Fail("No app key or access token");
                }
                var hasSession = await appService.HasSessionAsync(appKey, accessToken);
                if (!hasSession)
                {
                    return AuthenticateResult.Fail("No session token");
                }

                if (isApiQuery)
                {
                    var authToken = GetFromHeaderOrCookie(AuthHeader);
                    if (string.IsNullOrEmpty(authToken))
                    {
                        return AuthenticateResult.Fail("No authenticate!");
                    }
                    var tk = await userIdentityService.GetTokenInfoAsync(authToken);
                    if (tk is null)
                    {
                        return AuthenticateResult.Fail("No authenticate!");
                    }
                    context.Features.Set(tk);
                    var t = GetAuthTicket(tk.Name);
                    return AuthenticateResult.Success(t);
                }
            }            
            var tick = GetAuthTicket(string.Empty);
            return AuthenticateResult.Success(tick);
        }
        private AuthenticationTicket GetAuthTicket(string name)
        {
            var claimsIdentity = new ClaimsIdentity(new Claim[]
            {
                 new Claim(ClaimTypes.Name, name)
            }, SchemeName);

            var principal = new ClaimsPrincipal(claimsIdentity);
            return new AuthenticationTicket(principal, SchemeName);
        }
        public Task ChallengeAsync(AuthenticationProperties properties)
        {
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return Task.CompletedTask;
        }

        public Task ForbidAsync(AuthenticationProperties properties)
        {
            context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            return Task.CompletedTask;
        }
        public Task InitializeAsync(AuthenticationScheme scheme, HttpContext context)
        {
            this.context = context;
            isAppLogin = context.Request.Path.Value == AppController.LoginUri;
            isApiQuery = context.Request.Path.Value.StartsWith(PlanConst.ApiPrefx);
            if (scheme.Name == SkipAuthSchemeName)
            {
                isApiQuery = false;
            }
            return Task.CompletedTask;
        }
    }
}
