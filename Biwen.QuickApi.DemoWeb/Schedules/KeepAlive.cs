﻿using Biwen.QuickApi.Scheduling;

namespace Biwen.QuickApi.DemoWeb.Schedules
{
    /// <summary>
    /// KeepAlive ScheduleTask
    /// </summary>
    /// <param name="logger"></param>
    [ScheduleTask(Constants.CronEveryMinute)] //每分钟一次
    [ScheduleTask("0/3 * * * *")]//每3分钟执行一次
    public class KeepAlive(ILogger<KeepAlive> logger) : IScheduleTask
    {
        public async Task ExecuteAsync()
        {
            //执行5s
            await Task.Delay(TimeSpan.FromSeconds(5));
            logger.LogInformation("keep alive!");
        }
    }
}