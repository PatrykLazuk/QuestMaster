var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.QuestMaster_Server>("server")
    .WithHttpHealthCheck("/health");

builder.Build().Run();
