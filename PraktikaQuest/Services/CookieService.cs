using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;
using Messenger.Mongo;

namespace PraktikaQuest.Services
{
    public static class CookieService
    {
        public static async Task ValidateUser(CookieValidatePrincipalContext context)
        {
            var userContext = context.HttpContext.RequestServices.GetRequiredService<CollectionContext<User>>();

            if (context.Principal != null)
            {
                try
                {
                    string name = context.Principal.FindFirst(ClaimTypes.Name)?.Value ?? throw new ArgumentNullException("Name");

                    // Получаем пользователя по имени
                    var user = await userContext.FindFirstAsync(name);

                    if (user != null)
                    {
                        context.HttpContext.Items.Add("UserName", name);
                        return;
                    }
                }
                catch (ArgumentNullException)
                {
                    // Если возникает ошибка, отклоняем сессию
                    context.RejectPrincipal();
                }
            }
            context.RejectPrincipal();  // Отклоняем, если пользователя не найдено
        }
    }

    public static class CookieExtension
    {
        public static IServiceCollection AddCustomAuthentication(this IServiceCollection services)
        {
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(opt =>
                {
                    opt.Cookie.Name = "auth_info"; // Имя куки
                    opt.Cookie.HttpOnly = false;
                    opt.Cookie.MaxAge = TimeSpan.FromDays(30); // Время жизни куки
                    //opt.Events.OnValidatePrincipal = CookieService.ValidateUser; // Подключаем метод валидации
                    opt.AccessDeniedPath = "/Account/AccessDenied"; // Путь, если доступ запрещен
                    opt.LogoutPath = "/Account/Logout"; // Путь для выхода
                });
            return services;
        }
    }
}
