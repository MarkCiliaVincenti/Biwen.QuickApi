﻿namespace Biwen.QuickApi.Scheduling
{
    /// <summary>
    /// 任务调度接口
    /// </summary>
    public interface IScheduleTask
    {
        /// <summary>
        /// 任务执行
        /// </summary>
        /// <returns></returns>
        Task ExecuteAsync();
    }

    public abstract class ScheduleTask : IScheduleTask
    {
        public virtual async Task ExecuteAsync()
        {
            await Task.CompletedTask;
        }
    }

}