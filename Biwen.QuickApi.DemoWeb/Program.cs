using Biwen.QuickApi;
using Biwen.QuickApi.DemoWeb;
using Microsoft.OpenApi.Models;
using System.Text.Json;



var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication("Bearer");
builder.Services.AddAuthorization(builder => builder.AddPolicy("admin", policy => policy.RequireClaim("admin")));

//swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("1.0", new OpenApiInfo
    {
        Version = "1.0",
        Title = "API1����",
        Description = $"API����,{"1.0"}�汾, ?api-version=1.0"
    });

    options.SwaggerDoc("2.0", new OpenApiInfo
    {
        Version = "2.0",
        Title = "API2����",
        Description = $"API����,{"2.0"}�汾, ?api-version=2.0"
    });
});


// Add services to the container.
builder.Services.AddScoped<HelloService>();

//
builder.Services.AddBiwenQuickApis(o =>
{
    o.RoutePrefix = "quick";
    //����Ҫ�շ�ģʽ����Ϊnull
    //o.JsonSerializerOptions.PropertyNamingPolicy = null;
});

var app = builder.Build();


//swagger
app.UseSwagger();

app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint($"/swagger/1.0/swagger.json", "1.0");
    options.SwaggerEndpoint($"/swagger/2.0/swagger.json", "2.0");
});


app.UseAuthentication();
app.UseAuthorization();

//app.UseWelcomePage("/");
app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();


app.MapBiwenQuickApis();


//���������ط�����QuickApi
app.MapGet("/fromapi", async (Biwen.QuickApi.DemoWeb.Apis.JustAsService api) =>
{
    //ͨ����ķ�ʽ��ȡ�������
    var req = new EmptyRequest();
    //��֤�������
    var result = req.RealValidator.Validate(req);
    if (!result.IsValid)
    {
        return Results.BadRequest(result.ToDictionary());
    }

    //ִ������
    var x = await api.ExecuteAsync(new EmptyRequest());
    return Results.Content(x.ToString());
});


app.Run();
