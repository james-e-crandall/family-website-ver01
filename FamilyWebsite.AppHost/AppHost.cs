var builder = DistributedApplication.CreateBuilder(args);

// Add Keycloak authentication
var keycloak = builder.AddKeycloak("keycloak")
    .WithDataVolume();
    //.WithLifetime(ContainerLifetime.Persistent);

var authapi = builder.AddProject<Projects.AuthApi>("authapi")
    .WithReference(keycloak)
    .WaitFor(keycloak);

builder.AddJavaScriptApp("angular", "../family-app-ver01", runScriptName: "start")
    .WithReference(authapi)
    .WaitFor(authapi)
    .WithHttpEndpoint(env: "PORT")
    .WithExternalHttpEndpoints()
    .PublishAsDockerFile();


builder.Build().Run();
