using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddAuthentication(options =>
                {
                    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                })
                .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
                {
                    options.Cookie.Name = "__jec";
                    options.Cookie.SameSite = SameSiteMode.Strict;
                    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                })
                .AddKeycloakOpenIdConnect(
                    serviceName: "keycloak",
                    realm: "family",
                    options =>
                    {
                        options.ClientId = "family-api";
                        options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                        options.ResponseType = OpenIdConnectResponseType.Code;

                        options.SaveTokens = true;
                        options.GetClaimsFromUserInfoEndpoint = true;

                        options.MapInboundClaims = false;
                        options.TokenValidationParameters.NameClaimType = JwtRegisteredClaimNames.Name;
                        options.TokenValidationParameters.RoleClaimType = "roles";

                        options.CallbackPath = "/signin-oidc";
                        options.Scope.Add("family_api.all");

                        // For development only - disable HTTPS metadata validation
                        // In production, use explicit Authority configuration instead
                        if (builder.Environment.IsDevelopment())
                        {
                            options.RequireHttpsMetadata = false;
                        }                        
                    });

var app = builder.Build();

app.MapGet("/heath", () => "Hello World!");

app.Run();
