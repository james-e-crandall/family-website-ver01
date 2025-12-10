var builder = DistributedApplication.CreateBuilder(args);

// Add Keycloak authentication
var keycloak = builder.AddKeycloak("keycloak")
    .WithDataVolume();

var authapi = builder.AddProject<Projects.AuthApi>("authapi")
    .WithReference(keycloak)
    .WaitFor(keycloak);



builder.Build().Run();
