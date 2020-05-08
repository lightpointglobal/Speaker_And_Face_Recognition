using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpeechAndFaceRecognizerWebCore.Models;

namespace SpeechAndFaceRecognizerWebCore.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult EnterVacation(DateTime? dateStart, DateTime? dateEnd)
        {
            return View(new VacationViewModel {StartDate = dateStart, EndDate = dateEnd});
        }

        [HttpPost]
        public IActionResult EnterVacation(VacationViewModel model)
        {
            if(ModelState.IsValid)
                ViewBag.Success = 1;
            return View(model);
        }

        [HttpPost]
        public IActionResult ParseDate(string date)
        {
            date = date.Replace("года", "");
            if (DateTime.TryParse(date, out var parsed))
                return Json(new {success = 1, date = parsed.ToString("yyyy-MM-dd")});
            return Json(new { success = 0 });
        }
    }
}
