using Biwen.QuickApi;
using Biwen.QuickApi.DemoWeb;
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddBiwenQuickApis();
builder.Services.AddAuthentication("Bearer");
builder.Services.AddAuthorization(builder => builder.AddPolicy("admin", policy => policy.RequireClaim("admin")));

//swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// Add services to the container.
builder.Services.AddScoped<HelloService>();


var app = builder.Build();


app.UseAuthentication();
app.UseAuthorization();

app.UseWelcomePage("/");


//swagger
app.UseSwagger();
app.UseSwaggerUI();


app.MapBiwenQuickApis();


//���������ط�����QuickApi
app.MapGet("/fromapi", (Biwen.QuickApi.DemoWeb.Apis.Hello4Api api) =>
{
    //ͨ����ķ�ʽ��ȡ�������
    var req = new EmptyRequest();
    //��֤�������
    var validator = req.RealValidator as IValidator<EmptyRequest>;
    if (validator != null)
    {
        var result = validator.Validate(req);
        if (!result.IsValid)
        {
            return Results.BadRequest(result.ToDictionary());
        }
    }
    //ִ������
    var x = api.Execute(new EmptyRequest());
    return Results.Ok(x);
});


app.Run();
