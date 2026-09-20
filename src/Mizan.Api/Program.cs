var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => Results.Ok(new { service = "Mizan.Api", status = "foundation" }));

app.Run();

public partial class Program { }
