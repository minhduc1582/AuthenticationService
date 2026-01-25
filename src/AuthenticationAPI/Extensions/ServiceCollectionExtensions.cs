namespace AuthenticationAPI
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddOpenIddict()
                .AddServer(options =>
                {
                    // Enable the token endpoint.
                    options.SetAuthorizationEndpointUris("/connect/authorize")
                           .SetTokenEndpointUris("/connect/token")
                           .SetUserInfoEndpointUris("/connect/userinfo")
                           .SetEndSessionEndpointUris("/connect/endsession");

                    // Enable the client credentials flow.
                    options.AllowClientCredentialsFlow()
                            .AllowAuthorizationCodeFlow()
                            .RequireProofKeyForCodeExchange();

                    // Register the signing and encryption credentials.
                    options.AddDevelopmentEncryptionCertificate()
                           .AddDevelopmentSigningCertificate();

                    // Register the ASP.NET Core host and configure the ASP.NET Core options.
                    options.UseAspNetCore()
                            .EnableAuthorizationEndpointPassthrough()
                            .EnableEndSessionEndpointPassthrough()
                            .EnableTokenEndpointPassthrough();
                })
                .AddValidation(options =>
                 {
                     options.UseLocalServer();
                     options.UseAspNetCore();
                 });


            /// Service Dependencies Injection
            


            return services;
        }
    }
}
