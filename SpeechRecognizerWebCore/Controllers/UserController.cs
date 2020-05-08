using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SpeechAndFaceRecognizerWebCore.Application.Security;
using SpeechAndFaceRecognizerWebCore.Authentication;
using SpeechAndFaceRecognizerWebCore.Data;
using SpeechAndFaceRecognizerWebCore.Data.Entities;
using SpeechAndFaceRecognizerWebCore.Models;
using Universal.Microsoft.CognitiveServices.Face.V1;
using Universal.Microsoft.CognitiveServices.SpeakerRecognition.V1;

namespace SpeechAndFaceRecognizerWebCore.Controllers
{
    public class UserController : Controller
    {
        private readonly CookieAuthenticationService _authenticationService;
        private readonly DataContext _dataContext;
        private const string TfaUserIdSessionKey = "__two_factor_authentication_user_id";
        private const string TfaNextActionSessionKey = "__two_factor_authentication_next_action";
        private const string UnableRecognizeSpeechMessage = "Не удалось распознать голос";
        private readonly double _defaultMicrosoftEntrollmentSpeechTime = 30;

        public UserController(CookieAuthenticationService authenticationService, DataContext dataContext)
        {
            _authenticationService = authenticationService;
            _dataContext = dataContext;
        }
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(LoginViewModel model)
        {

            var user = _dataContext.Users.FirstOrDefault(u => u.Login == model.Login);
            if (user != null)
            {
                ModelState.AddModelError(nameof(model.Login), "Логин уже существует");

                return View(model);
            }

            if (model.Password.Length < 4)
            {
                ModelState.AddModelError(nameof(model.Password), "Пароль должен быть не менее 4 символов");

                return View(model);
            }

            var passwordHash = Password.CalculateHash(model.Password);

            user = new User
            {
                Login = model.Login,
                PasswordHash = passwordHash
            };
            _dataContext.Users.Add(user);
            _dataContext.SaveChanges();

            _authenticationService.SignIn(user.Id, true);

            return RedirectToAction("Index", "Home");
        }

        public IActionResult Logout()
        {
            _authenticationService.SignOut();
            return RedirectToAction("Login");
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {
            var passwordHash = Password.CalculateHash(model.Password);

            var user = _dataContext.Users.FirstOrDefault(u => u.PasswordHash == passwordHash && u.Login == model.Login);
            if (user == null)
            {
                ModelState.AddModelError(nameof(model.Password), "Неверный логин или пароль");

                return View(model);
            }

            _authenticationService.SignIn(user.Id, true);

            return RedirectToAction("Index", "Home");
        }

        public IActionResult Settings()
        {
            var user = _authenticationService.GetAuthenticatedUser();
            var speekerProfile = _dataContext.MicrosoftSpeekerIdentificationProfiles.FirstOrDefault(p => p.UserId == user.Id);
            _dataContext.MicrosoftFaceIdentificationPersonFaces.Load();
            var faceProfile = _dataContext.MicrosoftFaceIdentificationPersons.FirstOrDefault(p => p.UserId == user.Id);
            return View(new SettingsViewModel
            {
                Id = user.Id,
                Login = user.Login,
                MicrosoftFaceIdentificationProfileId = faceProfile?.Id,
                MicrosoftSpeekerIdentificationProfileId = speekerProfile?.Id,
                Faces = faceProfile?.Faces.Select(f=> (f.Id, $"data:image/png;base64,{Convert.ToBase64String(f.Data)}")),
                RemainingSpeechTime = speekerProfile?.RemainingSpeechTime
            });
        }

        [HttpPost]
        public IActionResult Settings(SettingsViewModel model)
        {
            var user = _dataContext.Users.FirstOrDefault(u => u.Id == _authenticationService.GetAuthenticatedUser().Id);
            if (user == null)
                return NotFound();
            user.Login = model.Login;
            if (!string.IsNullOrEmpty(model.Password))
                user.PasswordHash = Password.CalculateHash(model.Password);

            _dataContext.SaveChanges();

            return RedirectToAction("Index", "Home");
        }

        #region MicrosoftSpeekerRecognition + login

        [HttpPost]
        public async Task<IActionResult> SpeechLogin([FromServices] IConfiguration configuration, IFormFile sample)
        {
            var twoFactorEnabled = configuration.GetValue<bool>("TwoFactorAuthenticationEnabled");
            if (twoFactorEnabled)
            {
                HttpContext.Session.Remove(TfaNextActionSessionKey);
                HttpContext.Session.Remove(TfaUserIdSessionKey);
            }

            var profiles = _dataContext.MicrosoftSpeekerIdentificationProfiles.Select(p => p.Id).ToList();

            if(!profiles.Any())
                return Json(new { message = UnableRecognizeSpeechMessage });


            var speakerRecognitionClient = 
                new SpeakerRecognitionClient(
                    configuration["MicrosoftCognitiveServices:SpeekerRecognition:SubscriptionKey"],
                    configuration["MicrosoftCognitiveServices:SpeekerRecognition:Endpoint"]);

            try
            {
                byte[] byteData;
                using (var memoryStream = new MemoryStream())
                {
                    sample.CopyTo(memoryStream);
                    byteData = memoryStream.ToArray();
                }

                var chunks = profiles
                .Select((s, i) => new { Value = s.ToString(), Index = i })
                .GroupBy(x => x.Index / 100)
                .Select(grp => grp.Select(x => x.Value).ToArray())
                .ToArray();

                Guid? highConfidenceProfileId = null;
                Guid? normalConfidenceProfileId = null;
                foreach (var chunk in chunks)
                {
                    Universal.Microsoft.CognitiveServices.SpeakerRecognition.V1.IdentifyResponse result;
                    try
                    {
                        result = await speakerRecognitionClient.IdentifyAsync(chunk, byteData, true);
                    }
                    catch (Exception e)
                    {
                        if (e is SpeakerRecognitionException speakerRecognitionException && speakerRecognitionException.StatusCode == 400)
                            throw new AudioTooShortException("Аудио должно длиться хотя бы 1 секунду");
                        throw;
                    }

                    var operationResult =
                        await speakerRecognitionClient.PollOperationUntilCompleteAsync(result.OperationId, 1000);

                    if (operationResult.Status == "failed")
                        throw new Exception();

                    var identificationResult = operationResult.ProcessingResult.AsIdentificationResult();

                    var id = new Guid(identificationResult.IdentificationProfileId);

                    switch (identificationResult.Confidence)
                    {
                        case "High":
                            highConfidenceProfileId = id;
                            break;
                        case "Normal":
                            normalConfidenceProfileId = id;
                            break;
                    }

                    if (highConfidenceProfileId.HasValue)
                        break;
                }
                if(!highConfidenceProfileId.HasValue && !normalConfidenceProfileId.HasValue)
                    throw new Exception();

                var user = _dataContext.Users.FirstOrDefault(u =>
                    u.MicrosoftSpeekerIdentificationProfile.Id == highConfidenceProfileId || u.MicrosoftSpeekerIdentificationProfile.Id == normalConfidenceProfileId);

                if (user == null)
                    return Json(new { message = UnableRecognizeSpeechMessage });

                if (!highConfidenceProfileId.HasValue)
                {
                    if (twoFactorEnabled)
                    {
                        HttpContext.Session.SetString(TfaNextActionSessionKey, "face");
                        HttpContext.Session.SetString(TfaUserIdSessionKey, user.Id.ToString());

                        return Json(new {twoFactor = "face"});
                    }
                    return Json(new { message = UnableRecognizeSpeechMessage });
                }

                _authenticationService.SignIn(user.Id, true);

                return Json(new { redirect = "/" });


            }
            catch (AudioTooShortException)
            {
                return Json(new { message = "Не удалось войти. Проверьте, чтобы фраза была более 1 секунды" });
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddMicrosoftSpeekerIdentificationProfile([FromServices] IConfiguration configuration)
        {

            var user = _authenticationService.GetAuthenticatedUser();

            if (user.MicrosoftSpeekerIdentificationProfile != null)
                return Json(new {message = "У Вас уже есть профиль голосовой аутентификации"});

            var speakerRecognitionClient =
                new SpeakerRecognitionClient(
                    configuration["MicrosoftCognitiveServices:SpeekerRecognition:SubscriptionKey"],
                    configuration["MicrosoftCognitiveServices:SpeekerRecognition:Endpoint"]);
            try
            {
                var result = await speakerRecognitionClient.CreateIdentificationProfileAsync(new CreateIdentificationProfileRequest {Locale = "en-us"});
                var newProfile = new MicrosoftSpeekerIdentificationProfile
                {
                    Id = new Guid(result.IdentificationProfileId),
                    UserId = user.Id,
                    RemainingSpeechTime = _defaultMicrosoftEntrollmentSpeechTime
                };
                _dataContext.MicrosoftSpeekerIdentificationProfiles.Add(newProfile);
                _dataContext.SaveChanges();

                return Json(new {success = 1});
            }
            catch
            {
                return Json(new
                    {message = "Не удалось добавить голосовой профиль. Попробуйте повторить попытку позднее."});
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> DeleteMicrosoftSpeekerIdentificationProfile([FromServices]IConfiguration configuration)
        {
            var user = _authenticationService.GetAuthenticatedUser();
            var profile = _dataContext.MicrosoftSpeekerIdentificationProfiles.FirstOrDefault(p => p.UserId == user.Id);

            if (profile == null)
                return Json(new { message = "У Вас нет профиля голосовой аутентификации" });

            var speakerRecognitionClient =
                new SpeakerRecognitionClient(
                    configuration["MicrosoftCognitiveServices:SpeekerRecognition:SubscriptionKey"],
                    configuration["MicrosoftCognitiveServices:SpeekerRecognition:Endpoint"]);

            try
            {
                await speakerRecognitionClient.DeleteIdentificationProfileAsync(profile.Id.ToString());
                _dataContext.MicrosoftSpeekerIdentificationProfiles.Remove(profile);
                _dataContext.SaveChanges();
                return Json(new { success = 1 });
            }
            catch
            {
                return Json(new { message = "Не удалось удалить голосовой профиль. Попробуйте повторить попытку позднее." });
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> UploadMicrosoftSpeekerIdentificationEnrollmentSample([FromServices]IConfiguration configuration, IFormFile enrollmentSample)
        {
            var user = _authenticationService.GetAuthenticatedUser();
            var profile = _dataContext.MicrosoftSpeekerIdentificationProfiles.FirstOrDefault(p=>p.UserId == user.Id);
            if(profile == null)
                return Json(new { message = "Сначала добавьте профиль голосовой аутентификации" });

            var speakerRecognitionClient =
                new SpeakerRecognitionClient(
                    configuration["MicrosoftCognitiveServices:SpeekerRecognition:SubscriptionKey"],
                    configuration["MicrosoftCognitiveServices:SpeekerRecognition:Endpoint"]);
            byte[] byteData;
            using (var memoryStream = new MemoryStream())
            {
                enrollmentSample.CopyTo(memoryStream);
                byteData = memoryStream.ToArray();
            }
            try
            {
                CreateIdentificationProfileEnrollmentResponse result;

                try
                {
                    result = await speakerRecognitionClient.CreateIdentificationProfileEnrollmentAsync(profile.Id.ToString(), byteData, true);
                }
                catch (Exception e)
                {
                    if (e is SpeakerRecognitionException speakerRecognitionException && speakerRecognitionException.StatusCode == 400)
                        throw new AudioTooShortException("Аудио должно длиться хотя бы 1 секунду");
                    throw;
                }
                var operationResult =
                    await speakerRecognitionClient.PollOperationUntilCompleteAsync(result.OperationId, 1000);

                var enrollmentResult = operationResult.ProcessingResult.AsEnrollmentResult();

                var remainingSpeechTime = Math.Round(_defaultMicrosoftEntrollmentSpeechTime - enrollmentResult.EnrollmentSpeechTime, 2);

                if (profile.RemainingSpeechTime > 0)
                    profile.RemainingSpeechTime = remainingSpeechTime < 0 ? 0 : remainingSpeechTime;

                _dataContext.SaveChanges();
            }
            catch (AudioTooShortException)
            {
                return Json(new { message = "Не удалось провести обучение. Проверьте, чтобы фраза была более 1 секунды" });
            }
            catch
            { 
                return Json(new { message = "Не удалось провести обучение. Попробуйте повторить попытку позднее." });
            }

            return Json(new { success = 1 });
        }
        #endregion

        #region MicrosoftFaceRecognition + login

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> AddMicrosoftFaceIdentificationPersonGroup([FromServices] IConfiguration configuration)
        {
            var microsoftFaceIdentificationService = new FaceClient(configuration["MicrosoftCognitiveServices:Face:SubscriptionKey"], configuration["MicrosoftCognitiveServices:Face:Endpoint"]);
            try
            {
                var personGroup = new MicrosoftFaceIdentificationPersonGroup
                {
                    Name = "Group1"
                };
                _dataContext.MicrosoftFaceIdentificationPersonGroups.Add(personGroup);
                await _dataContext.SaveChangesAsync(true);
                await microsoftFaceIdentificationService.CreatePersonGroupAsync(personGroup.Id.ToString(),
                    new CreatePersonGroupRequest {Name = personGroup.Name, UserData = personGroup.UserData});
               

                return Json(new { success = 1 });
            }
            catch
            {
                return Json(new
                    { message = "Не удалось добавить группу пользователей. Попробуйте повторить попытку позднее." });
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddMicrosoftFaceIdentificationPerson([FromServices] IConfiguration configuration)
        {
            var personGroup = _dataContext.MicrosoftFaceIdentificationPersonGroups.FirstOrDefault();
            if(personGroup == null)
                return Json(new { message = "Сначала добавьте группу пользователей" });

            var user = _authenticationService.GetAuthenticatedUser();
            if(user.MicrosoftFaceIdentificationPerson != null)
                return Json(new { message = "У вас уже есть профиль аутентификации по фото лица" });

            var microsoftFaceIdentificationService = new FaceClient(configuration["MicrosoftCognitiveServices:Face:SubscriptionKey"], configuration["MicrosoftCognitiveServices:Face:Endpoint"]);
            try
            {
                var result = await microsoftFaceIdentificationService.CreatePersonGroupPersonAsync(personGroup.Id.ToString(),
                    new  CreatePersonGroupPersonRequest { Name = user.Login });

                var person = new MicrosoftFaceIdentificationPerson
                {
                    Id = new Guid(result.PersonId),
                    PersonGroupId = personGroup.Id,
                    UserId = user.Id,
                    Name = user.Login
                };
                _dataContext.MicrosoftFaceIdentificationPersons.Add(person);
                await _dataContext.SaveChangesAsync();

                return Json(new { success = 1 });
            }
            catch
            {
                return Json(new
                    { message = "Не удалось добавить профиль аутентификации по фото лица. Попробуйте повторить попытку позднее." });
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> DeleteMicrosoftFaceIdentificationPerson([FromServices] IConfiguration configuration)
        {
            var user = _authenticationService.GetAuthenticatedUser();
            var profile = _dataContext.MicrosoftFaceIdentificationPersons.FirstOrDefault(p => p.UserId == user.Id);

            if (profile == null)
                return Json(new { message = "У Вас нет профиля аутентификации по фото лица" });

            var microsoftFaceIdentificationService = new FaceClient(configuration["MicrosoftCognitiveServices:Face:SubscriptionKey"], configuration["MicrosoftCognitiveServices:Face:Endpoint"]);
            try
            {
               await microsoftFaceIdentificationService.DeletePersonGroupPersonAsync(profile.PersonGroupId.ToString(), profile.Id.ToString());

                _dataContext.MicrosoftFaceIdentificationPersons.Remove(profile);
                await _dataContext.SaveChangesAsync();

                return Json(new { success = 1 });
            }
            catch
            {
                return Json(new
                    { message = "Не удалось удалить профиль аутентификации по фото лица. Попробуйте повторить попытку позднее." });
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddMicrosoftFaceIdentificationPersonFace([FromServices] IConfiguration configuration, IFormFile photo)
        {
            var user = _authenticationService.GetAuthenticatedUser();
            var profile = _dataContext.MicrosoftFaceIdentificationPersons.FirstOrDefault(p => p.UserId == user.Id);
            if (profile == null)
                return Json(new { message = "У вас нет профиля аутентификации по фото лица" });

            var microsoftFaceIdentificationService = new FaceClient(configuration["MicrosoftCognitiveServices:Face:SubscriptionKey"], configuration["MicrosoftCognitiveServices:Face:Endpoint"]);
            byte[] byteData;
            using (var memoryStream = new MemoryStream())
            {
                photo.CopyTo(memoryStream);
                byteData = memoryStream.ToArray();
            }
            try
            {
                var response = await microsoftFaceIdentificationService.CreatePersonGroupPersonFaceAsync(profile.PersonGroupId.ToString(), profile.Id.ToString(), 
                    byteData, "");

                _dataContext.MicrosoftFaceIdentificationPersonFaces.Add(new MicrosoftFaceIdentificationPersonFace
                {
                    Id = new Guid(response.PersistedFaceId),
                    Data = byteData,
                    PersonId = profile.Id
                });
                _dataContext.SaveChanges();

                await microsoftFaceIdentificationService.TrainPersonGroupAsync(profile.PersonGroupId.ToString());

                return Json(new { success = 1 });
            }
            catch
            {
                return Json(new
                { message = "Не удалось добавить фото лица. Попробуйте повторить попытку позднее." });
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> DeleteMicrosoftFaceIdentificationPersonFace([FromServices] IConfiguration configuration, Guid photoId)
        {
            var user = _authenticationService.GetAuthenticatedUser();
            var profile = _dataContext.MicrosoftFaceIdentificationPersons.FirstOrDefault(p => p.UserId == user.Id);
            if (profile == null)
                return NotFound();

            var photo = _dataContext.MicrosoftFaceIdentificationPersonFaces.FirstOrDefault(p => p.PersonId == profile.Id && p.Id == photoId);
            if (photo == null)
                return NotFound();

            var microsoftFaceIdentificationService = new FaceClient(configuration["MicrosoftCognitiveServices:Face:SubscriptionKey"], configuration["MicrosoftCognitiveServices:Face:Endpoint"]);
           
            try
            {
                await microsoftFaceIdentificationService.DeletePersonGroupPersonFaceAsync(profile.PersonGroupId.ToString(), profile.Id.ToString(),
                    photoId.ToString());

                _dataContext.MicrosoftFaceIdentificationPersonFaces.Remove(photo);
                _dataContext.SaveChanges();

                await microsoftFaceIdentificationService.TrainPersonGroupAsync(profile.PersonGroupId.ToString());

                return Json(new { success = 1 });
            }
            catch
            {
                return Json(new
                    { message = "Не удалось удалить фото лица. Попробуйте повторить попытку позднее." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> FaceLogin([FromServices] IConfiguration configuration, IFormFile photo)
        {
            var twoFactorEnabled = configuration.GetValue<bool>("TwoFactorAuthenticationEnabled");
            if (twoFactorEnabled && HttpContext.Session.GetString(TfaNextActionSessionKey) != "face")
                return Json(new { message = "В данный момент функция недоступна" });
            var personGroup = _dataContext.MicrosoftFaceIdentificationPersonGroups.FirstOrDefault();
            if(personGroup == null)
                return Json(new { message = "В данный момент функция недоступна" });

            var microsoftFaceIdentificationService = new FaceClient(configuration["MicrosoftCognitiveServices:Face:SubscriptionKey"], configuration["MicrosoftCognitiveServices:Face:Endpoint"]);

            byte[] byteData;
            using (var memoryStream = new MemoryStream())
            {
                photo.CopyTo(memoryStream);
                byteData = memoryStream.ToArray();
            }

            try
            {
                var detectResult = await microsoftFaceIdentificationService.DetectAsync(byteData);
                if (detectResult.Count == 0)
                    return Json(new {message = "На фото не найдено ниодного лица"});
                if (detectResult.Count > 1)
                    return Json(new {message = "На фото найдено более 1 лица"});

                var identifyResult = await microsoftFaceIdentificationService.IdentifyAsync(new IdentifyRequest
                {
                    PersonGroupId = personGroup.Id.ToString(),
                    ConfidenceThreshold = 0.5,
                    MaxNumOfCandidatesReturned = 1,
                    FaceIds = detectResult.Select(c => c.FaceId).ToList()
                });

                var singleResult = identifyResult.FirstOrDefault();
                if (singleResult == null || singleResult.Candidates.Count == 0)
                    return Json(new {message = "Не найдено совпадений. Попробуйте другое фото."});
                if (singleResult.Candidates.Count > 1)
                    return Json(new {message = "Найдено более 1 совпадения. Попробуйте другое фото."});

                var singleCondidate = singleResult.Candidates.First();
                var personId = new Guid(singleCondidate.PersonId);
                var user = _dataContext.Users.FirstOrDefault(u => u.MicrosoftFaceIdentificationPerson.Id == personId);

                if (user == null || (twoFactorEnabled && user.Id.ToString() !=
                                     HttpContext.Session.GetString(TfaUserIdSessionKey)))
                    return Json(new {message = "Не удалось войти по фото лица"});

                _authenticationService.SignIn(user.Id, true);

                return Json(new {redirect = "/"});

            }
            catch
            {
                return Json(new {message = "Не удалось войти. Попробуйте повторить попытку позже."});
            }
        }
        #endregion
    }




    public class AudioTooShortException : Exception
    {
        public AudioTooShortException()
        {

        }

        public AudioTooShortException(string message) : base(message)
        {

        }
    }
}