using Microsoft.AspNetCore.Mvc;
using PraktikaQuest.Models;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PraktikaQuest.Controllers
{
    public class AccountController : Controller
    {
        private readonly RegistrAuthService _registrAuthService;

        public AccountController(RegistrAuthService registrAuthService)
        {
            _registrAuthService = registrAuthService;
        }

        // GET: /Account/Register
        [HttpGet("Account/Register")] // Явный маршрут для действия регистрации
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost("Account/Register")] 
        public async Task<IActionResult> Register(RegistrationModel model)
        {
            if (ModelState.IsValid)
            {
                bool isRegistered = await _registrAuthService.Registration(model);

                if (isRegistered)
                {
                    return RedirectToAction("Account/Login");
                }
                else
                {
                    // Добавляем ошибку в случае, если регистрация не удалась
                    ModelState.AddModelError(string.Empty, "Ошибка регистрации: имя пользователя уже занято или введены некорректные данные.");
                }
            }
            return View(model);
        }

        // GET: /Account/Login
        [HttpGet("Account/Login")] 
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost("Account/Login")] 
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (ModelState.IsValid)
            {
                bool isAuthenticated = await _registrAuthService.Auth(model);

                if (isAuthenticated)
                {
                    return Redirect("/game");

                }
                else
                {
                    // Добавляем ошибку, если авторизация не удалась
                    ModelState.AddModelError(string.Empty, "Ошибка авторизации: неправильное имя пользователя или пароль.");
                }
            }
            return View(model);
        }

        // GET: /Account/Logout
        [HttpGet("Account/Logout")] // Явный маршрут для действия выхода
        public async Task<IActionResult> Logout()
        {
            await _registrAuthService.LogOut();
            return RedirectToAction("Account/Login");
        }

        // Получение информации о текущем пользователе
        [HttpGet("Account/check-auth")]
        public IActionResult CheckAuth()
        {
            var user = HttpContext.User;
            var name = user.FindFirst(ClaimTypes.Name)?.Value;
            var id = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            Console.WriteLine($"CheckAuth - User: {name}, ID: {id}");
            return Ok(new { User = name, Id = id });
        }
    }
}
