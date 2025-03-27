using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Security.Policy;
using UPlant.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using UPlant.Models;
using static System.Net.WebRequestMethods;

namespace UPlant.Controllers
{
    public class AccountController : BaseController
    {
        private readonly IOptions<AppSettings> _opt;

        public AccountController( IOptions<AppSettings> opt)
        {
            
            _opt = opt;
            


        }


        public ActionResult AccessDenied()
        {
            return View();
        }
        public new SignOutResult SignOut()
        {
            var callbackUrl = Url.Action(nameof(SignedOut), "Account", values: null, protocol: Request.Scheme);
            return SignOut(
                new AuthenticationProperties { RedirectUri = callbackUrl },
                CookieAuthenticationDefaults.AuthenticationScheme,
                CookieAuthenticationDefaults.AuthenticationScheme);
        }
       
        [HttpGet]
        public async Task<IActionResult> SignedOut()
        {
            if (User.Identity.IsAuthenticated)
            {
               
                var identity = (ClaimsIdentity)User.Identity;

                
                var typeauth = _opt.Value.Application.TypeAuth;
                IEnumerable<Claim> claims = identity.Claims;
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity());


                var myToken = HttpContext.GetTokenAsync("access_token").Result ?? "";
                string LogoutUrl = "";
                if (identity.HasClaim("valorizzato", "Yes"))//parte di codice per Saml2 per fare la redirezione logout da perfezionare e configurare potrei settare in appsetting e passare la condfigurazione
                {
                    //da mettere nel config
                     LogoutUrl = "https://idp.unipi.it/logout.html";

                } else {
                    if (typeauth == "WSO2")
                    {
                        LogoutUrl = "https://iam.unipi.it/oidc/logout?id_token_hint=";
                        LogoutUrl = LogoutUrl + myToken;
                        if (!string.IsNullOrEmpty(LogoutUrl))
                        {
                            return Redirect($"{LogoutUrl}");
                        }
                        return BadRequest("Impossible to logout");
                    }
                    else if(typeauth == "AZURE")
                    {
                        var callbackUrl = Url.Action(nameof(SignedOut), "Account", values: null, protocol: Request.Scheme);
                        return SignOut(
                            new AuthenticationProperties { RedirectUri = callbackUrl },
                            CookieAuthenticationDefaults.AuthenticationScheme,
                            OpenIdConnectDefaults.AuthenticationScheme);
                    }
                    else if (typeauth == "SAML2")
                    {
                        //da rivedere
                        LogoutUrl = "https://iam.unipi.it/oidc/logout?id_token_hint=";
                        LogoutUrl = LogoutUrl + myToken;
                        if (!string.IsNullOrEmpty(LogoutUrl))
                        {
                            return Redirect($"{LogoutUrl}");
                        }
                        return BadRequest("Impossible to logout");
                    }
                }
                
                //var allDomainCookes = HttpContext.Request.Cookies.Keys;

                //foreach (string domainCookie in allDomainCookes)
                //{
                //    HttpContext.Response.Cookies.Delete(domainCookie);
                //}
                //return Content("Logout eseguito");
            }

            return View();
        }
    }
}
