using BenchmarkTestWeb;
using Biwen.QuickApi;
using Biwen.QuickApi.Swagger;
using Microsoft.AspNetCore.Mvc;
using NSwag;
using NSwag.Annotations;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddQuickApiDocument(options =>
{
    options.UseControllerSummaryAsTagDescription = true;
    options.DocumentName = "Quick API ALL";

    //options.ApiGroupNames = new[] { };//δָ��չʾȫ��Api

    options.PostProcess = document =>
    {
        document.Info = new OpenApiInfo
        {
            Version = "Quick API ALL",
            Title = "Quick API testcase",
            Description = "Biwen.QuickApi ��������",
            TermsOfService = "https://github.com/vipwan",
            Contact = new OpenApiContact
            {
                Name = "��ӭ Star & issue",
                Url = "https://github.com/vipwan/Biwen.QuickApi"
            },
            License = new OpenApiLicense
            {
                Name = "MIT License",
                Url = "https://github.com/vipwan/Biwen.QuickApi/blob/master/LICENSE.txt"
            }
        };
    };
},
new SecurityOptions());

builder.Services.AddBiwenQuickApis(o =>
{
    o.RoutePrefix = "";
});


var app = builder.Build();

if (!app.Environment.IsProduction())
{
    app.UseDeveloperExceptionPage();
    //swagger ui
    app.UseQuickApiSwagger(uiConfig: cfg =>
    {
        cfg.DefaultModelsExpandDepth = -1;
    });

    app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();
}
else
{
    app.UseWelcomePage("/");
}

app.Use(async (context, next) =>
{
    //ͳһ�ȴ�10΢��
    //await Task.Delay(TimeSpan.FromMicroseconds(10));
    //��¼��־
    Console.WriteLine($"{context.Request.Path.Value} {context.Request.Host.Value}:{context.Request.ContentType}");
    await next(context);
});

//minimal
app.MapPost("/my-minimal", ([Microsoft.AspNetCore.Mvc.FromBody] MyRequest request) =>
{
    var validResult = request.Validate();
    if (!validResult.IsValid)
    {
        return Results.ValidationProblem(validResult.ToDictionary());
    }
    return Results.Ok(request);
}).WithTags("API");


//default��ʽ
app.MapBiwenQuickApis();

app.UseAuthorization();
app.MapControllers();

app.Run();

namespace BenchmarkTestWeb
{
    public partial class Program { }
}