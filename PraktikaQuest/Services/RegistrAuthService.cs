using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Mvc;
using Messenger.Mongo;
using System.Security.Claims;
using PraktikaQuest.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson;


public class RegistrAuthService
{
    private readonly CollectionContext<User> UserContext;
    private readonly IHttpContextAccessor ContextAccessor;
    private readonly CollectionContext<GameState> _gamestates;

    public RegistrAuthService(CollectionContext<User> userContext, IHttpContextAccessor contextAccessor, CollectionContext<GameState> gamestates)
    {
        UserContext = userContext;
        ContextAccessor = contextAccessor;
        _gamestates = gamestates;
    }

    // Регистрация пользователя
    public async Task<bool> Registration(RegistrationModel regModel)
    {
        // Проверка на уникальность имени пользователя
        if (await UserContext.FindByAnyFieldAsync("Name", regModel.Name) != null)
        {
            // Возвращаем false, если имя пользователя уже существует
            return false;
        }

        // Проверка на пустое имя
        if (string.IsNullOrEmpty(regModel.Name))
        {
            // Возвращаем false, если имя пустое
            return false;
        }

        // Создание нового пользователя
        var userData = new User
        {
            Name = regModel.Name,
        };

        // Сохранение пользователя в базу данных
        await UserContext.InsertDocument(userData);
        var user = await UserContext.FindByAnyFieldAsync("Name", userData.Name);
        var game = new GameState
        {
            PlayerId = user.Id,
            CurrentBlockId = 1
        };
        await _gamestates.InsertDocument(game);

        return true;
    }

    // Авторизация пользователя
    public async Task<bool> Auth(LoginModel logModel)
    {
        // Получаем пользователя по имени
        var user = await UserContext.FindByAnyFieldAsync("Name", logModel.Name);
     
        // Если пользователь не найден, возвращаем false
        if (user == null)
        {
            return false;
        }

        // Создание клеймов и вход в систему
        await CreateClaimsAndSignIn(user);
        return true;
    }

    // Создание клеймов и вход в систему
    public async Task CreateClaimsAndSignIn(User user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
        };


        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

        await ContextAccessor.HttpContext.SignInAsync("Cookies", claimsPrincipal);
    }

    // Получение имени текущего пользователя
    public string GetCurrentUserName()
    {
        var userClaim = ContextAccessor.HttpContext.User;
        var userName = userClaim.FindFirst(ClaimTypes.Name)?.Value;
        return userName;
    }

    // Получение Id текущего пользователя
    public ObjectId? GetCurrentUserId()
    {
        var userIdClaim = ContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim != null && ObjectId.TryParse(userIdClaim, out ObjectId userId))
        {
            return userId;
        }
        return null;
    }

    // Выход из системы
    public async Task LogOut()
    {
        await ContextAccessor.HttpContext.SignOutAsync("Cookies");
    }
}

