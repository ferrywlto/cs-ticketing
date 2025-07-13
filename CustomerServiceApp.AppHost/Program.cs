var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.CustomerServiceApp_API>("apiservice");

builder.AddProject<Projects.CustomerServiceApp_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService);

builder.Build().Run();
