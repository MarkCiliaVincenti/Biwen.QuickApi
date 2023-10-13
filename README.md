﻿# Biwen.QuickApi

## 项目介绍

```csharp
[QuickApi("hello/world")]
public class MyApi : BaseQuickApi<Req,Rsp>{}
``` 
- (MinimalApi as REPR) Biwen.QuickApi遵循了 REPR 设计 （Request-Endpoint-Response）
- 开箱即用的Route, Policy,Binder,validator & 整合NSwag支持
- 该库是NET WebApi/Minimal Api的补充，性能≈MinimalApi(gen版本=minimalApi,生成原生接口代码),遥遥领先于MVC和WebApi，但是提供了最简单的的使用体验
- write less, do more ; write anywhere, do anything  
- 欢迎小伙伴们star&issue共同学习进步 (Biwen.QuickApi)[https://github.com/vipwan/Biwen.QuickApi]


## SourceGenerator
- 提供gen源代码生成器方案,以于显著提升性能(V1.0版本使用的Emit和dynamic会导致部分性能损失)
- gen SourceGenerator已发布v1.1.2,[使用方式](https://github.com/vipwan/Biwen.QuickApi/blob/master/Biwen.QuickApi.Generator/readme.md)

## 使用方式

### Step0 Nuget Base & Generator
```bash
dotnet add package Biwen.QuickApi
```
```bash
dotnet add package Biwen.QuickApi.SourceGenerator
```
### Step1 UseBiwenQuickApis

```csharp

builder.Services.AddBiwenQuickApis();

//缺省方案
app.MapBiwenQuickApis();

//Gen方案
//app.MapGenBiwenQuickApis();
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
        /// <summary>
        /// FromQuery特性绑定字段
        /// </summary>
        [FromQuery("q")]
        public string? Q { get; set; }
        public HelloApiRequest()
        {
            RuleFor(x => x.Name).NotNull().Length(5, 10);
        }
    }
    
    /// <summary>
    /// 上传文件FileUploadRequest 
    /// </summary>
    public class FileUploadRequest : BaseRequest<FileUploadRequest>
    {
        public IFormFile? File { get; set; }

        public FileUploadRequest()
        {
            RuleFor(x => x.File).NotNull();
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
    /// 标记FromBody,表示这个请求对象是FromBody的
    /// </summary>
    [FromBody]
    public class FromBodyRequest : BaseRequest<FromBodyRequest>
    {
        public int Id { get; set; }
        public string? Name { get; set; }

        public FromBodyRequest()
        {
            RuleFor(x => x.Id).InclusiveBetween(1, 100);//必须1~100
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
            //自定义绑定器
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
            //默认实现了Accepts和Produces
            return base.HandlerBuilder(builder);
            //如果完全自定义直接返回Builder
            //return builder;
         }
    }

     /// <summary>
    /// 提供对IResult的封装支持
    /// </summary>
    [QuickApi("iresult", Verbs = Verb.GET)]
    public class IResultTestApi : BaseQuickApiWithoutRequest<IResultResponse>
    {
        public override async Task<IResultResponse> ExecuteAsync(EmptyRequest request)
        {
            return Results.Ok("Hello World IResult!").AsRsp();
        }

        public override RouteHandlerBuilder HandlerBuilder(RouteHandlerBuilder builder)
        {
            //针对IResultResponse,需要完全自定义Produces,QuickApi无法自动识别
            builder.Produces(200, typeof(string), contentType: "text/plain");
            return builder;
            //return base.HandlerBuilder(builder);
        }
    }
    

    /// <summary>
    /// 上传文件测试
    /// 请使用postman & apifox 测试
    /// </summary>
    [QuickApi("fromfile", Verbs = Verb.POST)]
    public class FromFileApi : BaseQuickApi<FileUploadRequest, IResultResponse>
    {
        public override async Task<IResultResponse> ExecuteAsync(FileUploadRequest request)
        {
            //测试上传一个文本文件并读取内容
            if (request.File != null)
            {
                using (var sr = new StreamReader(request.File.OpenReadStream()))
                {
                    var content = await sr.ReadToEndAsync();
                    return Results.Ok(content).AsRsp();
                }
            }
            return Results.BadRequest("no file").AsRsp();
        }

        public override RouteHandlerBuilder HandlerBuilder(RouteHandlerBuilder builder)
        {
            builder.Accepts<FileUploadRequest>("multipart/form-data");
            builder.WithOpenApi(operation => new(operation)
            {
                Summary = "上传文件测试",
                Description = "上传文件测试"
            });
            return builder;
        }
    }

    /// <summary>
    /// JustAsService 只会被服务发现，不会被注册到路由表
    /// </summary>
    [QuickApi(""), JustAsService]
    public class JustAsService : BaseQuickApi<EmptyRequest, ContentResponse>
    {
        public override Task<ContentResponse> ExecuteAsync(EmptyRequest request)
        {
            return Task.FromResult(new ContentResponse("Hello World JustAsService!"));
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

### Step5 NSwag集成 

```c#

//register nswag & quickapi document

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


//use swagger ui

app.UseOpenApi();
app.UseSwaggerUi3();


```




### Step6 OpenApi 以及Client代理

- 你可以全局配置版本号,以及自定义的OpenApi描述
- 你可以重写QuickApi的HandlerBuilder方法,以便于你自定义的OpenApi描述
- 我们强烈建议您使用Refit生成代理代码,以便于您的客户端和服务端保持一致的接口定义
- 因为遵循REPR风格,所以不推荐SwaggerUI或使用SwaggerStudio生成代理代码,除非您的QuickApi定义的相当规范(如存在自定义绑定,别名绑定等)!

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

app.MapGet("/from-quickapi", async (IBusiness bussiness) =>
{
    var resp = await bussiness.TestPost();
    return Results.Content(resp.Message);
});

```
### Q&A

- 为什么不支持.NET6
-- 考虑到.NET8已经发布了RC,11月就会发布RTM,这个是未来3年的长期支持版本,并且.NET是可以无缝升级到.NET8的,所以我没有计划.NET6支持,直接拥抱新特性

- 为什么不支持多个参数的绑定?
-- 因为我认为这样的Api设计是不合理的,我们遵循REPR设计理念,如果你需要多个参数,请使用复杂化的Request对象

- QuickApi中如何拿到HttpContext对象?
-- 请在构造函数中注入IHttpContextAccessor获取

- 是否支持Minimal的中间件和拦截器?
-- 支持的,本身QuickApi就是扩展了MinimalApi,底层也是Minimal的处理机制,所以请考虑全局的中间件和拦截器,以及重写QuickApi的HandlerBuilder方法