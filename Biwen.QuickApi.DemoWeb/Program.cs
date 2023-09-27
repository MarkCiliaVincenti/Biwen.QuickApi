using Biwen.QuickApi;
using Biwen.QuickApi.DemoWeb;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Writers;

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


// var apis = app.MapBiwenQuickApis();
//
app.MapGenQuickApis("api");
//���������ض��ķ�����������. ������Ȩ��,����������

//���������ط�����QuickApi
app.MapGet("/fromapi", async (JustAsService api) =>
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



using var scopeNeedAuthApi = app.Services.CreateScope();


var mapNeedAuthApi = app.MapMethods("admin/index", new[] { "GET", "POST" }, async (IHttpContextAccessor ctx, NeedAuthApi api) =>
{
    //��֤����
    var policy = "admin";
    if (!string.IsNullOrEmpty(policy))
    {
        var httpContext = ctx.HttpContext;
        var authService = httpContext!.RequestServices.GetService<IAuthorizationService>() ?? throw new QuickApiExcetion($"IAuthorizationService is null,besure services.AddAuthorization() first!");
        var authorizationResult = await authService.AuthorizeAsync(httpContext.User, policy);
        if (!authorizationResult.Succeeded)
        {
            return Results.Unauthorized();
        }
    }
    //�󶨶���
    var req = await api.ReqBinder.BindAsync(ctx.HttpContext!);

    //��֤��
    if (req.RealValidator.Validate(req) is ValidationResult vresult && !vresult!.IsValid)
    {
        return Results.ValidationProblem(vresult.ToDictionary());
    }
    //ִ������
    try
    {
        var result = await api.ExecuteAsync(req!);
        return Results.Json(result);
    }
    catch (Exception ex)
    {
        var exceptionHandlers = ctx.HttpContext!.RequestServices.GetServices<IQuickApiExceptionHandler>();
        //�쳣����
        foreach (var handler in exceptionHandlers)
        {
            await handler.HandleAsync(ex);
        }
        //Ĭ�ϴ���
        throw;
    }
});
//handler
scopeNeedAuthApi.ServiceProvider.GetRequiredService<NeedAuthApi>().HandlerBuilder(mapNeedAuthApi);



app.Run();
