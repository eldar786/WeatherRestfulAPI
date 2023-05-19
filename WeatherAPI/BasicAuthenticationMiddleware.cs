using Microsoft.AspNetCore.Http;
using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WeatherAPI
{
    public class BasicAuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly UsersRepository _usersRepository;

        public BasicAuthenticationMiddleware(RequestDelegate next, UsersRepository usersRepository)
        {
            _next = next;
            _usersRepository = usersRepository;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            if (!Authenticate(context.Request))
            {
                context.Response.Headers.Add("WWW-Authenticate", "Basic");
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return;
            }

            await _next(context);
        }
        private bool Authenticate(HttpRequest request)
        {
            string authorizationHeader = request.Headers["Authorization"];

            if (authorizationHeader != null && authorizationHeader.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
            {
                var encodedUsernamePassword = authorizationHeader.Substring("Basic ".Length).Trim();
                var usernamePassword = Encoding.UTF8.GetString(Convert.FromBase64String(encodedUsernamePassword));
                var parts = usernamePassword.Split(':');
                var username = parts[0];
                var password = parts[1];

                var user = _usersRepository.GetUser(username);

                if (user != null && user.Password == password)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
