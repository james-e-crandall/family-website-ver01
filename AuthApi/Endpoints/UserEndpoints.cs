using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http.HttpResults;
namespace AuthApi.Endpoints;

internal static partial class UserEndpoints
{
    extension(HttpContext context)
    {
        public string BuildRedirectUrl(string? redirectUrl)
        {
            if (string.IsNullOrEmpty(redirectUrl))
            {
                redirectUrl = "/";
            }
            if (redirectUrl.StartsWith('/'))
            {
                redirectUrl = context.Request.Scheme + "://" + context.Request.Host + context.Request.PathBase + redirectUrl;
            }
            return redirectUrl;
        }
    }

    internal static IEndpointRouteBuilder RegisterUserEndpoints(this IEndpointRouteBuilder builder)
    {

        builder.MapGet("user", (ClaimsPrincipal principal) =>
        {
            var user = principal switch
            {
                { Identity.IsAuthenticated: true } => new LoginUserDto
                {
                    IsAuthenticated = true,
                    Name = principal.FindFirstValue("name"),
                },
                _ => new LoginUserDto
                {
                    IsAuthenticated = false,
                    Name = null
                }
            };

            return TypedResults.Ok(user);
        }).RequireAuthorization();

        builder.MapGet("login", (string? returnUrl, string? claimsChallenge, HttpContext context, HttpRequest request) =>
        {
            if(string.IsNullOrEmpty(returnUrl))
            {
                returnUrl = request.Headers["Referer"].ToString();
            }
            var properties = new AuthenticationProperties
            {
                RedirectUri = context.BuildRedirectUrl(returnUrl),
            };
            Console.WriteLine($"Login RedirectUri: {properties.RedirectUri}");
            if (claimsChallenge != null)
            {
                string jsonString = claimsChallenge.Replace("\\", "", StringComparison.Ordinal).Trim(['"']);
                Console.WriteLine($"Login Claims Challenge: {jsonString}");
                properties.Items["claims"] = jsonString;
            }

            return TypedResults.Challenge(properties);
        }).AllowAnonymous();

        builder.MapGet("logout", async (HttpContext context, string? returnUrl) =>
        {
            // Retrieve the ID token from the current session properties
            var authResult = await context.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            var items = authResult?.Properties?.Items ?? new Dictionary<string, string?>();
            Console.WriteLine($"Logout Auth Items: {items.Count}");
            foreach(var item in items)
            {
                Console.WriteLine($"Logout Auth Item: {item.Key} - {item.Value}");
            }

            var idToken = authResult?.Properties?.GetTokenValue("id_token");
            if(string.IsNullOrEmpty(idToken))
            {
                authResult = await context.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                items = authResult?.Properties?.Items ?? new Dictionary<string, string?>();
                Console.WriteLine($"Logout Auth Items: {items.Count}");
                foreach(var item in items)
                {
                    Console.WriteLine($"Logout Auth Item: {item.Key} - {item.Value}");
                }

                idToken = authResult?.Properties?.GetTokenValue("id_token");
            }
            if(string.IsNullOrEmpty(idToken))
            {
                foreach(var item in context.Request.Cookies)
                {
                    Console.WriteLine($"Logout Auth Item: {item.Key} - {item.Value}");
                }
            }

            Console.WriteLine($"Logout ID Token: {idToken}");

            var properties = new AuthenticationProperties { RedirectUri = returnUrl };
            if (idToken != null)
            {
                // Explicitly set the id_token_hint parameter
                properties.SetParameter("id_token_hint", idToken);
            }
            else
            {
                properties.SetParameter("id_token_hint", string.Empty);
            }

            return TypedResults.SignOut(properties, [
                CookieAuthenticationDefaults.AuthenticationScheme,
                OpenIdConnectDefaults.AuthenticationScheme
            ]);
        });

        return builder;
    }
}