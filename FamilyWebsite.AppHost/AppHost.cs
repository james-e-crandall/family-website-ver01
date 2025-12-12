using Aspire.Hosting.Yarp.Transforms;

var builder = DistributedApplication.CreateBuilder(args);

// Add Keycloak authentication
var keycloak = builder.AddKeycloak("keycloak", 8181)
    .WithDataVolume();
    //.WithLifetime(ContainerLifetime.Persistent);

var authapi = builder.AddProject<Projects.AuthApi>("authapi")
    .WithReference(keycloak)
    .WaitFor(keycloak);

var angularFamilyApp = builder.AddJavaScriptApp("angularFamilyApp", "../family-app-ver01", runScriptName: "start")
    .WithReference(authapi)
    .WaitFor(authapi)
    .WithHttpEndpoint(env: "PORT")
    .WithExternalHttpEndpoints()
    .PublishAsDockerFile();

var gateway = builder.AddYarp("gateway")
    .WithHostPort(8080)
    .WithReference(keycloak)
    .WaitFor(keycloak)
    .WithReference(authapi)
    .WaitFor(authapi)
    .WithReference(angularFamilyApp)
    .WaitFor(angularFamilyApp)
    .WithConfiguration(yarp =>
    {
        // Keycloak reverse proxy
        yarp.AddRoute("/keycloak/{**catch-all}", keycloak)
            .WithTransformPathRemovePrefix("/keycloak");
        yarp.AddRoute("/authapi/{**catch-all}", authapi)
            .WithTransformPathRemovePrefix("/authapi");
        yarp.AddRoute("/{**catch-all}", angularFamilyApp);
    });

authapi.WithReference(gateway);
angularFamilyApp.WithReference(gateway);

builder.Build().Run();
