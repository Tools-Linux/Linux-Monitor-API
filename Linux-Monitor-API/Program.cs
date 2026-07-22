using Linux_Monitor_API.Services.CPU;
using Linux_Monitor_API.Services.Disk;
using Linux_Monitor_API.Services.Logs;
using Linux_Monitor_API.Services.Memory;
using Linux_Monitor_API.Websocket;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://0.0.0.0:5000");

builder.Services.AddControllers();
builder.Services.AddSignalR();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .WithOrigins(
                "http://192.168.1.130:5173",
                "http://localhost:5173"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});


builder.Services.AddSingleton<CpuServices>();
builder.Services.AddSingleton<MemoryServices>();
builder.Services.AddSingleton<DashboardWebSocket>();
builder.Services.AddSingleton<LogsWebsocket>();
builder.Services.AddSingleton<LogsServices>();
builder.Services.AddSingleton<DiskServices>();
builder.Services.AddSingleton<Linux_Monitor_API.Services.Services.ProcessManager>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
        
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();

app.UseCors("Frontend");

app.UseAuthorization();

app.UseWebSockets();

app.Map("/ws/dashboard", async context =>
{
    if (!context.WebSockets.IsWebSocketRequest)
    {
        context.Response.StatusCode = 400;
        return;
    }

    var socket = await context.WebSockets.AcceptWebSocketAsync();

    var dashboard = context.RequestServices
        .GetRequiredService<DashboardWebSocket>();

    await dashboard.HandleAsync(socket, context.RequestAborted);
});

app.Map("/ws/logs", async context =>
{
    if(!context.WebSockets.IsWebSocketRequest)
    {
        context.Response.StatusCode = 400;
        return;
    }
    
    var socket = await context.WebSockets.AcceptWebSocketAsync();

    var logs = context.RequestServices
        .GetRequiredService<LogsWebsocket>();

    await logs.HandleAsync(
        socket,
        context.RequestAborted
    );
});

app.Map("/ws/services", async context =>
{
    if(!context.WebSockets.IsWebSocketRequest)
    {
        context.Response.StatusCode = 400;
        return;
    }
    
    var socket = await context.WebSockets.AcceptWebSocketAsync();

    var ServiceWebsocket = context.RequestServices
        .GetRequiredService<ServiceWebsocket>();

    await ServiceWebsocket.HandleAsync(
        socket,
        context.RequestAborted
    );
});

app.MapControllers();

app.Run();