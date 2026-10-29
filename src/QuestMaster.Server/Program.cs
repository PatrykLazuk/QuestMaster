var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

var app = builder.Build();

// /health and /alive, mapped only in the Development environment (see ServiceDefaults).
app.MapDefaultEndpoints();

app.Run();
