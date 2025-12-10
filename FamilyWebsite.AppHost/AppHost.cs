var builder = DistributedApplication.CreateBuilder(args);

// Add Keycloak authentication
var keycloak = builder.AddKeycloak("keycloak")
    .WithDataVolume();

builder.Build().Run();
