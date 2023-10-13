using Biwen.QuickApi.DemoWeb;
using Biwen.QuickApi.DemoWeb.Apis;
using Biwen.QuickApi.SourceGenerator;
using Biwen.QuickApi.Swagger;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NSwag;
using System.Text.Json.Serialization;
using System.Text.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication().AddCookie();
builder.Services.AddAuthorization();

builder.Services.AddOutputCache(options =>
{
    options.AddBasePolicy(builder => builder.Expire(TimeSpan.FromSeconds(10)));
});

builder.Services.AddResponseCaching();

//builder.Services.AddAuthorizationBuilder().AddPolicy("admin", policy =>
//{
//    policy.RequireClaim("admin");
//    policy.RequireAuthenticatedUser();
//});

builder.Services.Configure<AuthorizationOptions>(options =>
{
    options.AddPolicy("admin", policy =>
    {
        policy.RequireClaim("admin");
        policy.RequireAuthenticatedUser();
    });
});

builder.Services.Configure<AuthenticationOptions>(options =>
{
    options.DefaultScheme = "Cookies";
    options.DefaultChallengeScheme = "Cookies";
});

//swagger
builder.Services.AddQuickApiDocument(options =>
{
    options.UseControllerSummaryAsTagDescription = true;
    options.PostProcess = document =>
    {
        document.Info = new OpenApiInfo
        {
            Version = "Quick API Demo V1",
            Title = "Quick API Demo",
            Description = "Biwen.QuickApi Demo",
            TermsOfService = "https://github.com/vipwan",
            Contact = new OpenApiContact
            {
                Name = "Contact Me",
                Url = "https://github.com/vipwan/Biwen.QuickApi"
            },
            License = new OpenApiLicense
            {
                Name = "MIT License",
                Url = "https://github.com/vipwan/Biwen.QuickApi/blob/master/LICENSE.txt"
            }
        };
    };
});


// Add services to the container.
builder.Services.AddScoped<HelloService>();
// keyed services
builder.Services.AddKeyedScoped<HelloService>("hello");

//
builder.Services.AddBiwenQuickApis(o =>
{
    o.RoutePrefix = "quick";
    //����Ҫ�շ�ģʽ����Ϊnull
    //o.JsonSerializerOptions.PropertyNamingPolicy = null;
});

var app = builder.Build();

//swagger
app.UseOpenApi();
app.UseSwaggerUi3();


app.UseAuthentication();
app.UseAuthorization();

app.UseOutputCache();
app.UseResponseCaching();

// Ĭ�Ϸ�ʽ
var apis = app.MapBiwenQuickApis();
//���������ض��ķ�����������. ������Ȩ��,����������,����ע��ò����Ḳ�ǵ�ԭ�е�����(������ڵ������)
var groupAdmin = apis.FirstOrDefault(x => x.Group == "admin");
groupAdmin.RouteGroupBuilder?
    .WithTags("Admin Test")         //�Զ���Tags
    .RequireHost("localhost:5101") //ģ����Ҫָ��Host���ʽӿ�
    ;

// Gen��ʽ
//app.MapGenQuickApis(app.Services);

//app.UseWelcomePage("/");
app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();


//���������ط�����QuickApi
app.MapGet("/fromapi", async Task<Results<Ok<string>, BadRequest<IDictionary<string, string[]>>>>
    (JustAsService api) =>
{
    //ͨ����ķ�ʽ��ȡ�������
    var req = new EmptyRequest();
    //��֤�������
    var result = req.RealValidator.Validate(req);
    if (!result.IsValid)
    {
        return TypedResults.BadRequest(result.ToDictionary());
    }

    //ִ������
    var x = await api.ExecuteAsync(req);
    return TypedResults.Ok(x.Content);

}).RequireAuthorization("admin");

//����ms��WithOpenApi��һ��BUG,��MethodΪ���ʱ�ᱨ��!
//app.MapMethods("hello-world", new[] { "GET", "POST" }, () => Results.Ok()).WithOpenApi(operation => new(operation)
//{
//    Summary = "NeedAuthApi",
//    Description = "NeedAuthApi"
//});

app.Run();
