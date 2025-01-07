using MV_test.Utils;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSearchService();
builder.Services.AddSeedService();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(setup => { 
    setup.MapType<DateTime>(() => new Microsoft.OpenApi.Models.OpenApiSchema
    {
        Type = "string",
        Format = "date",
        Example = new Microsoft.OpenApi.Any.OpenApiString("dd.MM.yyyy")
    }); 
});

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "MV Test");
        c.RoutePrefix = string.Empty;
    });
}

app.MapControllers();

app.Run();

