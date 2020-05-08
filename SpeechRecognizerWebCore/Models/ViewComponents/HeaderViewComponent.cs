using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SpeechAndFaceRecognizerWebCore.Authentication;

namespace SpeechAndFaceRecognizerWebCore.Models.ViewComponents
{
    public class HeaderViewComponent : ViewComponent
    {
        private readonly CookieAuthenticationService _authenticationService;

        public HeaderViewComponent(CookieAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var user = _authenticationService.GetAuthenticatedUser();

            var viewModel = new HeaderViewModel
            {
                UserIsAuthenticated = user != null,
                UserName = user?.Login

            };
            return View(viewModel);
        }
    }
}
