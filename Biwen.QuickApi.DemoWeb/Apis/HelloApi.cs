﻿using Biwen.QuickApi.Attributes;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;


#pragma warning disable

namespace Biwen.QuickApi.DemoWeb.Apis
{
    public class HelloApiRequest : BaseRequest<HelloApiRequest>
    {
        public string? Name { get; set; } = "default";

        /// <summary>
        /// 别名测试
        /// </summary>
        [AliasAs("a")]
        public string? Alias { get; set; }

        [FromQuery]
        public string? Q { get; set; }

        public HelloApiRequest()
        {
            RuleFor(x => x.Name).NotNull().Length(2, 36);
        }
    }


    /// <summary>
    /// 标记FromBodyReq,表示这个请求对象是FromBody的
    /// </summary>
    [FromBodyReq]
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
    public class CustomApiRequestBinder : IReqBinder<HelloApiRequest>
    {
        public async Task<HelloApiRequest> BindAsync(HttpContext context)
        {
            var request = new HelloApiRequest();

            //支持默认值,如果没有c,则使用默认值
            if (context.Request.Query.TryGetValue("c", out var c))
            {
                request.Name = c;
            }

            await Task.CompletedTask;
            return request;
        }
    }


    public class HelloApiResponse : BaseResponse
    {
        public string? Message { get; set; }

        /// <summary>
        /// 返回字段的别名测试
        /// </summary>
        ///[AliasAs("a")]
        [JsonPropertyName("a")]
        public string? Alias { get; set; }
    }


    [QuickApi("index", Group = "admin", Verbs = Verb.GET, Policy = "admin")]
    public class NeedAuthApi : BaseQuickApi
    {
        public override Task<EmptyResponse> ExecuteAsync(EmptyRequest request)
        {
            return Task.FromResult(EmptyResponse.New);
        }


        public override RouteHandlerBuilder HandlerBuilder(RouteHandlerBuilder builder)
        {
            builder.WithOpenApi(operation => new(operation)
            {
                Summary = "NeedAuthApi",
                Description = "NeedAuthApi"
            });

            return base.HandlerBuilder(builder);
        }

    }

    /// <summary>
    /// get ~/hello/world/{name}
    /// </summary>
    [QuickApi("world/{name}", Group = "hello", Verbs = Verb.GET | Verb.POST)]
    public class HelloApi : BaseQuickApi<HelloApiRequest, HelloApiResponse>
    {
        public override async Task<HelloApiResponse> ExecuteAsync(HelloApiRequest request)
        {
            await Task.CompletedTask;
            return new HelloApiResponse
            {
                Message = $"Hello {request.Name} {request.Q}",
                Alias = request.Alias,
            };
        }
    }


    /// <summary>
    /// Post ~/hello/world2
    /// </summary>
    [QuickApi("world2", Group = "hello", Verbs = Verb.POST)]
    public class Hello2Api : BaseQuickApi<HelloApiRequest, HelloApiResponse>
    {
        public override async Task<HelloApiResponse> ExecuteAsync(HelloApiRequest request)
        {
            await Task.CompletedTask;
            return new HelloApiResponse
            {
                Message = $"Hello {request.Name}  {request.Alias} ",
                Alias = request.Alias //别名测试 Alias -> a
            };
        }

        public override RouteHandlerBuilder HandlerBuilder(RouteHandlerBuilder builder)
        {
            builder.WithOpenApi(operation => new(operation)
            {
                Summary = "world2",
                Description = "world2"
            });

            return base.HandlerBuilder(builder);
        }


    }

    /// <summary>
    /// Post ~/hello/world3
    /// </summary>

    [QuickApi("world3", Group = "hello", Verbs = Verb.POST)]
    public class Hello3Api : BaseQuickApi<EmptyRequest, HelloApiResponse>
    {
        public override async Task<HelloApiResponse> ExecuteAsync(EmptyRequest request)
        {
            await Task.CompletedTask;
            return new HelloApiResponse
            {
                Message = $"Hello 3"
            };
        }
    }

    /// <summary>
    /// 注入服务 ~/hello/world4
    /// </summary>

    [QuickApi("world4", Group = "hello", Verbs = Verb.GET)]
    public class Hello4Api : BaseQuickApi<EmptyRequest, HelloApiResponse>
    {
        private readonly HelloService _service;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public Hello4Api(HelloService service, IHttpContextAccessor httpContextAccessor)
        {
            _service = service;
            _httpContextAccessor = httpContextAccessor;
        }

        public override async Task<HelloApiResponse> ExecuteAsync(EmptyRequest request)
        {
            await Task.CompletedTask;
            var hello = _service.Hello($"hello world {_httpContextAccessor.HttpContext!.Request.Path} !");
            return new HelloApiResponse
            {
                Message = hello
            };
        }

    }

    /// <summary>
    /// 默认不需要Group
    /// </summary>
    [QuickApi("world5", Verbs = Verb.GET)]
    public class Hello5Api : BaseQuickApi
    {
        public override async Task<EmptyResponse> ExecuteAsync(EmptyRequest request)
        {
            await Task.CompletedTask;
            return EmptyResponse.New;
        }
    }

    /// <summary>
    /// 默认不需要Group
    /// </summary>
    [QuickApi("world6", Verbs = Verb.GET)]
    public class Hello6Api : BaseQuickApiWithoutRequest<HelloApiResponse>
    {
        public override async Task<HelloApiResponse> ExecuteAsync(EmptyRequest request)
        {
            await Task.CompletedTask;
            return new HelloApiResponse
            {
                Message = "Hello 6",
                Alias = "Alias"
            };
        }
    }

    /// <summary>
    /// get ~/custom?c=11112222
    /// </summary>
    [QuickApi("custom", Verbs = Verb.GET)]
    public class CustomApi : BaseQuickApi<HelloApiRequest>
    {
        public CustomApi()
        {
            UseReqBinder<CustomApiRequestBinder>();
        }

        public override async Task<EmptyResponse> ExecuteAsync(HelloApiRequest request)
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
            //自定义标签
            builder.WithTags("custom");

            //自定义过滤器
            builder.AddEndpointFilter(async (context, next) =>
            {
                Console.WriteLine("自定义过滤器!");
                return await next(context);
            });

            builder.WithOpenApi(operation => new(operation)
            {
                Summary = "custom",
                Description = "custom"
            });

            return base.HandlerBuilder(builder);
        }



    }

    #region 版本控制测试

    [QuickApi("v1")]
    public class V1Api : BaseQuickApi
    {
        public override Task<EmptyResponse> ExecuteAsync(EmptyRequest request)
        {
            Console.WriteLine("v1");
            return Task.FromResult(EmptyResponse.New);
        }

        public override RouteHandlerBuilder HandlerBuilder(RouteHandlerBuilder builder)
        {
            builder.WithTags("VT");//按照版本分组
            return base.HandlerBuilder(builder);
        }
    }

    [QuickApi("v1v2")]
    public class V1V2Api : BaseQuickApi
    {
        public override Task<EmptyResponse> ExecuteAsync(EmptyRequest request)
        {
            Console.WriteLine("v1,v2");
            return Task.FromResult(EmptyResponse.New);
        }

        public override RouteHandlerBuilder HandlerBuilder(RouteHandlerBuilder builder)
        {

            builder.WithTags("VT");//按照版本分组

            return base.HandlerBuilder(builder);
        }
    }

    [QuickApi("v2")]
    public class V2Api : BaseQuickApi
    {
        public override Task<EmptyResponse> ExecuteAsync(EmptyRequest request)
        {
            Console.WriteLine("v2");
            return Task.FromResult(EmptyResponse.New);
        }

        public override RouteHandlerBuilder HandlerBuilder(RouteHandlerBuilder builder)
        {
            builder.WithTags("VT");//按照版本分组

            return base.HandlerBuilder(builder);
        }
    }

    #endregion

    /// <summary>
    /// get ~/content 返回文本测试
    /// </summary>
    [QuickApi("content", Group = "hello", Verbs = Verb.GET)]
    public class ContentApi : BaseQuickApi<EmptyRequest, ContentResponse>
    {
        public override Task<ContentResponse> ExecuteAsync(EmptyRequest request)
        {
            return Task.FromResult(new ContentResponse("Hello World content!"));
        }

        public override RouteHandlerBuilder HandlerBuilder(RouteHandlerBuilder builder)
        {
            return base.HandlerBuilder(builder);
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


    [QuickApi("frombody", Verbs = Verb.POST)]
    public class FromBodyApi : BaseQuickApi<FromBodyRequest, ContentResponse>
    {
        public override async Task<ContentResponse> ExecuteAsync(FromBodyRequest request)
        {
            return new ContentResponse($"FromBodyApi {request.Id} {request.Name}");
        }
    }
}

#pragma warning restore