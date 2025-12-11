var builder = DistributedApplication.CreateBuilder(args);

// Add Redis cache
var redis = builder.AddRedis("cache");

var apiService = builder.AddProject<Projects.AspireTest_ApiService>("apiservice")
    .WithHttpHealthCheck("/health")
    .WithReference(redis);

builder.AddProject<Projects.AspireTest_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(apiService)
    .WithReference(redis)
    .WaitFor(apiService);

builder.Build().Run();
