using Biwen.QuickApi.DemoWeb;
using Biwen.QuickApi.DemoWeb.Apis;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication().AddCookie();
builder.Services.AddAuthorization();

builder.Services.AddOutputCache();

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

//builder.Services.ConfigureApplicationCookie(options =>
//{
//    options.LoginPath = "/login";
//});

//swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


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
app.UseSwagger();
app.UseSwaggerUI();


app.UseAuthentication();
app.UseAuthorization();

app.UseOutputCache();

// Ĭ�Ϸ�ʽ
var apis = app.MapBiwenQuickApis();
//���������ض��ķ�����������. ������Ȩ��,����������,����ע��ò����Ḳ�ǵ�ԭ�е�����(������ڵ������)
var groupAdmin = apis.FirstOrDefault(x => x.Group == "admin");
groupAdmin.RouteGroupBuilder?
    .WithTags("Admin Test")
    .WithOpenApi(operation => new(operation)
    {
        Summary = "���ڲ���Ȩ�����",
        //Description = "Admin Test"
    })                             //�Զ���OpenApi
    .RequireHost("localhost:5101") //ģ����Ҫָ��Host���ʽӿ�
    ;

// Gen��ʽ
//app.MapGenQuickApis(app.Services);

//app.UseWelcomePage("/");
app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();


//���������ط�����QuickApi
app.MapGet("/fromapi",
    async Task<Results<Ok<string>, BadRequest<IDictionary<string, string[]>>>>
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

//app.MapGet("hhe", () => TypedResults.Ok(new EmptyResponse()));


//����ms��WithOpenApi��һ��BUG,��MethodΪ���ʱ�ᱨ��!
//app.MapMethods("hello-world", new[] { "GET", "POST" }, () => Results.Ok()).WithOpenApi(operation => new(operation)
//{
//    Summary = "NeedAuthApi",
//    Description = "NeedAuthApi"
//});



app.Run();
