using TelemetryWeb.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSignalR();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

app.UseStaticFiles();
app.UseCors();
app.UseRouting();

app.MapControllers();
app.MapHub<AlertHub>("/alertHub");

app.MapGet("/", () => Results.Redirect("/index.html"));

app.Run("http://0.0.0.0:5050");
