using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using SpeechAndFaceRecognizerWebCore.Data;
using SpeechAndFaceRecognizerWebCore.Data.Entities;

namespace SpeechAndFaceRecognizerWebCore.Authentication
{
    public class CookieAuthenticationService 
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly DataContext _context;


        public CookieAuthenticationService(IHttpContextAccessor httpContextAccessor, DataContext context)
        {
            _httpContextAccessor = httpContextAccessor;
            _context = context;
        }
       
        public void SignIn(Guid userId, bool isPersistent)
        {
            if (userId == null || userId == Guid.Empty)
                throw new ArgumentNullException(nameof(userId));

            var userIdentity = new ClaimsIdentity(new List<Claim> {new Claim(ClaimTypes.Name, userId.ToString(), ClaimValueTypes.String,  "_speech_recognition_service") },
                CookieAuthenticationDefaults.AuthenticationScheme);

            var userPrincipal = new ClaimsPrincipal(userIdentity);

            var authCookieExpires = DateTime.UtcNow.AddDays(7);
            var authenticationProperties = new AuthenticationProperties
            {
                IsPersistent = isPersistent,
                ExpiresUtc = authCookieExpires
            };

             _httpContextAccessor.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, userPrincipal, authenticationProperties);
        }
        public void SignOut()
        {
            _httpContextAccessor.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }

        public User GetAuthenticatedUser()
        {
            var authenticateResult = _httpContextAccessor.HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme).Result;
            if (!authenticateResult.Succeeded)
                return null;

            var idClaim = authenticateResult.Principal.FindFirst(claim => claim.Type == ClaimTypes.Name);
            if (idClaim != null)
            {
                var id = new Guid(idClaim.Value);
                var user = _context.Users.FirstOrDefault(u => u.Id == id);
                if (user != null)
                    return user;

                SignOut();
            }
            else
                return null;

            return null;
        }
    }
}