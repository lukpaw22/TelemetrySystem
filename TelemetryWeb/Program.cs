using SignalRApp.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSignalR();

// CORS potrzebne gdy frontend odpytuje z innego portu
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

app.UseStaticFiles();   // serwuj pliki z wwwroot/
app.UseCors();
app.UseRouting();

// Kontrolery – webhook od InfluxDB trafi tutaj
app.MapControllers();

// Hub SignalR – klienci łączą się pod /alertHub
app.MapHub<AlertHub>("/alertHub");

// Przekieruj / na index.html
app.MapGet("/", () => Results.Redirect("/index.html"));

app.Run();
