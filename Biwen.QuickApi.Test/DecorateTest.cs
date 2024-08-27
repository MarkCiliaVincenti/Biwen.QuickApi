﻿using Microsoft.Extensions.Logging;

namespace Biwen.QuickApi.Test
{

    public class DecorateTest(ITestOutputHelper testOutput)
    {
        [Fact]
        public async Task TestDecorate()
        {
            var services = new ServiceCollection();
            services.AddSingleton<ITestService, TestService>();
            services.Decorate<ITestService, DecorateTestService>();
            var provider = services.BuildServiceProvider();
            var service = provider.GetRequiredService<ITestService>();
            var result = await service.SayHello();

            testOutput.WriteLine(result);

            Assert.Equal("Decorated: hello world", result);
        }

        [Fact]
        public async Task TestDecorate_with_di_params()
        {
            var services = new ServiceCollection();
            services.AddSingleton<ITestService, TestService>();

            //通过注入的方式:
            services.Decorate<ITestService, DecorateTestService2>();
            services.AddLogging(builder => builder.AddConsole());


            var provider = services.BuildServiceProvider();
            var service = provider.GetRequiredService<ITestService>();
            var result = await service.SayHello();
            testOutput.WriteLine(result);
            Assert.Equal("Decorated2: hello world", result);
        }


        [Fact]
        public async Task TestDecorate_with_input_params()
        {
            var services = new ServiceCollection();
            services.AddSingleton<ITestService, TestService>();

            //通过传参的方式:
            ILogger<DecorateTestService> logger = new LoggerFactory().CreateLogger<DecorateTestService>();

            services.Decorate<ITestService, DecorateTestService2>(logger);

            var provider = services.BuildServiceProvider();
            var service = provider.GetRequiredService<ITestService>();
            var result = await service.SayHello();
            testOutput.WriteLine(result);
            Assert.Equal("Decorated2: hello world", result);
        }

        interface ITestService
        {
            Task<string?> SayHello();
        }

        class TestService : ITestService
        {
            public async Task<string?> SayHello()
            {
                await Task.CompletedTask;
                return "hello world";
            }
        }

        /// <summary>
        /// 不含其他注入的情况
        /// </summary>
        /// <param name="inner"></param>
        class DecorateTestService(ITestService inner) : ITestService
        {
            public async Task<string?> SayHello()
            {
                var old = await inner.SayHello();
                return $"Decorated: {old}";
            }
        }

        /// <summary>
        /// 其他参数注入的情况
        /// </summary>
        /// <param name="inner"></param>
        /// <param name="logger"></param>
        class DecorateTestService2(ITestService inner, ILogger<DecorateTestService> logger) : ITestService
        {
            public async Task<string?> SayHello()
            {
                var old = await inner.SayHello();

                logger.LogInformation(old);

                return $"Decorated2: {old}";
            }
        }


    }
}