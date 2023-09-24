﻿# Biwen.QuickApi

## 项目介绍

```csharp
[QuickApi("hello/world")]
public class MyApi : BaseQuickApi<Req,Rsp>{}
``` 
- 提供一种简单集成的Minimal Web Api交互模块 遵循了 REPR 设计 （Request-Endpoint-Response）
- 开箱即用的Api路由 和 权限,Bind,validator体验
- 该库是NET WebApi/Minimal Api的补充，性能≈MinimalApi,遥遥领先于MVC和WebApi，但是提供了最简单的的使用体验
- write less, do more ; write anywhere, do anything  
- 欢迎小伙伴们star&issue共同学习进步 (Biwen.QuickApi)[https://github.com/vipwan/Biwen.QuickApi]


## 使用方式

### Step0 Nuget Install
```bash
dotnet add package Biwen.QuickApi
```

### Step1 UseBiwenQuickApis

```csharp

builder.Services.AddBiwenQuickApis(o =>
{
    o.RoutePrefix = "quick";
    //不需要驼峰模式设置为null
    //o.JsonSerializerOptions.PropertyNamingPolicy = null;
});

//....
app.MapBiwenQuickApis();

```

### Step2 Define Request and Response

```csharp

    public class HelloApiRequest : BaseRequest<HelloApiRequest>
    {
        public string? Name { get; set; }

        /// <summary>
        /// 别名绑定字段
        /// </summary>
        [AliasAs("a")]
        public string? Alias { get; set; }

        public HelloApiRequest()
        {
            RuleFor(x => x.Name).NotNull().Length(5, 10);
        }
    }

    /// <summary>
    /// 模拟自定义绑定的Request
    /// </summary>
    public class CustomApiRequest : BaseRequest<CustomApiRequest>
    {
        public string? Name { get; set; }

        public CustomApiRequest()
        {
            RuleFor(x => x.Name).NotNull().Length(5, 10);
        }
    }

    /// <summary>
    /// 自定义的绑定器
    /// </summary>
    public class CustomApiRequestBinder : IReqBinder<CustomApiRequest>
    {
        public async Task<CustomApiRequest> BindAsync(HttpContext context)
        {
            var request = new CustomApiRequest
            {
                Name = context.Request.Query["c"]
            };
            await Task.CompletedTask;
            return request;
        }
    }

    public class HelloApiResponse : BaseResponse
    {
        public string? Message { get; set; }
    }

```

### Step3 Define QuickApi

```csharp

    /// <summary>
    /// get ~/admin/index
    /// </summary>
    [QuickApi("index", Group = "admin", Verbs = Verb.GET | Verb.POST, Policy = "admin")]
    public class NeedAuthApi : BaseQuickApi
    {
        public override EmptyResponse Execute(EmptyRequest request)
        {
            return EmptyResponse.Instance;
        }
    }

    /// <summary>
    /// get ~/hello/world/{name}
    /// </summary>
    [QuickApi("world/{name}", Group = "hello", Verbs = Verb.GET | Verb.POST)]
    public class HelloApi : BaseQuickApi<HelloApiRequest, HelloApiResponse>
    {
        private readonly HelloService _service;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public Hello4Api(HelloService service,IHttpContextAccessor httpContextAccessor)
        {
            _service = service;
            _httpContextAccessor = httpContextAccessor;
        }

        public override HelloApiResponse Execute(HelloApiRequest request)
        {
            var hello = _service.Hello($"hello world {_httpContextAccessor.HttpContext!.Request.Path} !");
            return new HelloApiResponse
            {
                Message = hello
            };
        }
    }

    /// <summary>
    /// get ~/custom?c=11112222
    /// </summary>
    [QuickApi("custom", Verbs = Verb.GET)]
    public class CustomApi : BaseQuickApi<CustomApiRequest>
    {
        public CustomApi()
        {
            UseReqBinder<CustomApiRequestBinder>();
        }

        public override async Task<EmptyResponse> ExecuteAsync(CustomApiRequest request)
        {
            await Task.CompletedTask;
            Console.WriteLine($"获取自定义的 CustomApi:,从querystring:c绑定,{request.Name}");
            return EmptyResponse.New;
        }

        /// <summary>
        /// 提供minimal扩展
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public override RouteHandlerBuilder HandlerBuilder(RouteHandlerBuilder builder)
        {
            //自定义描述
            builder.WithOpenApi(operation => new(operation)
            {
                Summary = "This is a summary",
                Description = "This is a description"
            });

            //自定义标签
            builder.WithTags("custom");

            //自定义过滤器
            builder.AddEndpointFilter(async (context, next) =>
            {
                Console.WriteLine("自定义过滤器!");
                return await next(context);
            });

            //自定义Api版本
            //默认为版本1.0,如果需要访问其他版本,需要在querystring中添加?api-version=2.0 :)
            builder.HasApiVersion(1.0).WithGroupName("1.0");
            builder.HasApiVersion(2.0).WithGroupName("2.0");

            return builder;
        }

    }


```


### Step4 Enjoy !

```csharp

//直接访问
// GET ~/hello/world/biwen
// GET ~/hello/world/biwen?name=biwen
// POST ~/hello/world/biwen
// GET ~/custom?c=11112222

```

```csharp

//你也可以把QuickApi当Service使用
app.MapGet("/fromapi", async (Biwen.QuickApi.DemoWeb.Apis.Hello4Api api) =>
{
    //通过你的方式获取请求对象
    var req = new EmptyRequest();
    //验证请求对象
    var result = req.RealValidator.Validate(req);
    if (!result.IsValid)
    {
        return Results.BadRequest(result.ToDictionary());
    }
    //执行请求
    var x = await api.ExecuteAsync(new EmptyRequest());
    return Results.Ok(x);
});

```

### Step5 OpenApi 以及Client代理

- 你可以全局配置版本号,以及自定义的OpenApi描述
- 你可以重写QuickApi的HandlerBuilder方法,以便于你自定义的OpenApi描述
- 我们强烈建议您使用Refit生成代理代码,以便于您的客户端和服务端保持一致的接口定义
- 不推荐您使用SwaggerStudio生成代理代码,除非您的QuickApi定义的相当规范!

```csharp

/// <summary>
/// refit client
/// </summary>
public interface IBusiness
{
    [Refit.Get("/fromapi")]
    public Task<TestRsp> TestPost();
}

//Refit
builder.Services.AddRefitClient<IBusiness>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri("http://localhost:5101"));

var app = builder.Build();

app.MapGet("/from-quickapi", async (IRefitBusiness bussiness) =>
{
    var resp = await bussiness.TestPost();
    return Results.Content(resp.Message);
});

```
### Q&A

- 为什么不支持多个参数的绑定?
-- 因为我认为这样的Api设计是不合理的,我们遵循REPR设计理念,如果你需要多个参数,请使用复杂化的Request对象

- QuickApi中如何拿到HttpContext对象?
-- 请在构造函数中注入IHttpContextAccessor获取

- 是否支持Minimal的中间件和拦截器?
-- 支持的,本身QuickApi就是扩展了MinimalApi,底层也是Minimal的处理机制,所以请考虑全局的中间件和拦截器,以及重写QuickApi的HandlerBuilder方法