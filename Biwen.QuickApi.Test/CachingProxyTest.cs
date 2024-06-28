﻿using Biwen.QuickApi.Caching;
using Microsoft.Extensions.DependencyInjection;

namespace Biwen.QuickApi.Test
{
    public class CachingProxyTest(ITestOutputHelper testOutput)
    {
        [Fact]
        public async Task SampleTest()
        {
            var services = new ServiceCollection();
            services.AddLogging();

            //测试拦截TestClass
            var testClass = new TestClass();
            var decored = CachingProxy<ITestClass>.Create(testClass);

            //测试拦截效果:
            decored.TestMethod();
            await decored.TestTask();


            //由于命中了缓存,因此返回都是相同:
            var time1 = decored.TestMethod2(1, "123");
            testOutput.WriteLine(time1.ToString());
            await Task.Delay(3000);
            var time2 = decored.TestMethod2(1, "123");
            testOutput.WriteLine(time2.ToString());
            Assert.Equal(time1, time2);


            //测试缓存过期:
            var time3 = await decored.TestTask2(1);
            testOutput.WriteLine(time3.ToString());
            await Task.Delay(6000);
            var time4 = await decored.TestTask2(1);
            testOutput.WriteLine(time4.ToString());
            Assert.NotEqual(time3, time4);

            await Task.CompletedTask;
        }
    }


    public interface ITestClass
    {
        [AutoCache]
        void TestMethod();

        [AutoCache]
        Task TestTask();

        [AutoCache]
        DateTime TestMethod2(int random, string random2);

        [AutoCache(5)]
        Task<DateTime> TestTask2(int random);
    }

    public class TestClass : ITestClass
    {
        public void TestMethod()
        {
            Console.WriteLine("Hello World");
        }

        public Task TestTask()
        {
            return Task.CompletedTask;
        }

        public DateTime TestMethod2(int random, string random2)
        {
            return DateTime.Now;
        }

        public Task<DateTime> TestTask2(int random)
        {
            return Task.FromResult(DateTime.Now);
        }
    }
}