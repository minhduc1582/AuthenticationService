var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();


var app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.MapFallbackToFile("index.html");

app.Run();
