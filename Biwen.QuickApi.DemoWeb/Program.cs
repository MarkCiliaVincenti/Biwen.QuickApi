using Biwen.QuickApi.DemoWeb;
using Biwen.QuickApi.DemoWeb.Apis;
using Biwen.QuickApi.DemoWeb.GroupRouteBuilders;
using Biwen.QuickApi.Swagger;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NSwag;
using NSwag.AspNetCore;
using System.Text;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication().AddCookie();
builder.Services.AddAuthorization();

builder.Services.AddOutputCache(options =>
{
    //options.AddBasePolicy(builder => builder.Expire(TimeSpan.FromSeconds(10)));
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

#region swagger �ĵ�


//swagger
builder.Services.AddQuickApiDocument(options =>
{
    options.UseControllerSummaryAsTagDescription = true;
    options.DocumentName = "Quick API ALL QuickApi";

    //options.ApiGroupNames = new[] { };//δָ��չʾȫ��Api

    options.PostProcess = document =>
    {
        document.Info = new OpenApiInfo
        {
            Version = "Quick API ALL QuickApi",
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
new SecurityOptions(), true);

builder.Services.AddQuickApiDocument(options =>
{
    options.UseControllerSummaryAsTagDescription = true;
    options.DocumentName = "Quick API Admin&Group1";

    options.ApiGroupNames = new[] { "admin", "group1" }; //�ĵ�����ָ��

    options.PostProcess = document =>
    {
        document.Info = new OpenApiInfo
        {
            Version = "Quick API V2",
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


#endregion




// Add services to the container.
builder.Services.AddScoped<HelloService>();
// keyed services
//builder.Services.AddKeyedScoped<HelloService>("hello");

//
builder.Services.AddBiwenQuickApis(o =>
{
    o.RoutePrefix = "quick";
    o.EnableAntiForgeryTokens = true;
});

//�����Ҫ�Զ������·�ɹ�����
builder.Services.AddBiwenQuickApiGroupRouteBuilder<DefaultGroupRouteBuilder>();
//�����Ҫ�Զ����쳣���ظ�ʽ
//builder.Services.AddSingleton<IQuickApiExceptionResultBuilder, CustomExceptionResultBuilder>();
//�Զ����쳣����
builder.Services.AddScoped<IQuickApiExceptionHandler, CustomExceptionHandler>();

var app = builder.Build();


if (!app.Environment.IsProduction())
{
    app.UseDeveloperExceptionPage();

    //swagger ui
    app.UseQuickApiSwagger(uiConfig: o =>
    {
        //o.CustomJavaScriptPath = "/miniprofiler-sc";
    });

    app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

}
else
{
    app.UseWelcomePage("/");
}

app.UseAuthentication();
app.UseAuthorization();

app.UseOutputCache();
app.UseResponseCaching();

// Ĭ�Ϸ�ʽ
app.MapBiwenQuickApis();

// Gen��ʽ
//app.MapGenQuickApis(app.Services);

//���������ط�����QuickApi
app.MapGet("/fromapi", async Task<Results<Ok<string>, BadRequest<IDictionary<string, string[]>>>>
    (JustAsService api) =>
{
    //ͨ����ķ�ʽ��ȡ�������
    var req = new EmptyRequest();
    //��֤�������
    var result = req.Validate();
    if (!result.IsValid)
    {
        return TypedResults.BadRequest(result.ToDictionary());
    }

    //ִ������
    var x = await api.ExecuteAsync(req);
    return TypedResults.Ok(x.Content);

});

//����ms��WithOpenApi��һ��BUG,��MethodΪ���ʱ�ᱨ��!
//��ֱ��ʹ��QuickApiSummaryAttribute!
//app.MapMethods("hello-world", new[] { "GET", "POST" }, () => Results.Ok()).WithOpenApi(operation => new(operation)
//{
//    Summary = "NeedAuthApi",
//    Description = "NeedAuthApi"
//});


app.Run();


//����xunit Test
namespace Biwen.QuickApi.DemoWeb
{
    public partial class Program { }
}