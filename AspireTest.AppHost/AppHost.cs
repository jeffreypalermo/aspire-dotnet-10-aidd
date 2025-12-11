var builder = DistributedApplication.CreateBuilder(args);

// Add Redis cache
var redis = builder.AddRedis("cache");

// Add SQL Server with a default database
var sqlServer = builder.AddSqlServer("sqlserver")
    .WithLifetime(ContainerLifetime.Persistent)
    .AddDatabase("catalogdb");

var apiService = builder.AddProject<Projects.AspireTest_ApiService>("apiservice")
    .WithHttpHealthCheck("/health")
    .WithReference(redis);

builder.AddProject<Projects.AspireTest_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(apiService)
    .WithReference(redis)
    .WithReference(sqlServer)
    .WaitFor(apiService)
    .WaitFor(sqlServer);

builder.Build().Run();
