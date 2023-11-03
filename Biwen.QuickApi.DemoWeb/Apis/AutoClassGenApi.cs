﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Biwen.QuickApi.DemoWeb.Apis
{
    /// <summary>
    /// 分页
    /// </summary>
    public interface IPager
    {
        /// <summary>
        /// 页码
        /// </summary>
        [DefaultValue(0), Description("页码,从0开始")]
        [Range(0, int.MaxValue)]
        int? CurrentPage { get; set; }

        /// <summary>
        /// 分页项数
        /// </summary>
        [DefaultValue(10), Description("每页项数,10-30之间")]
        [Range(10, 30)]
        int? PageLen { get; set; }

    }

    /// <summary>
    /// 查询
    /// </summary>
    public interface IQuery
    {
        /// <summary>
        /// 关键字
        /// </summary>
        [StringLength(100), Description("查询关键字")]
        string? KeyWord { get; set; }
    }

    /// <summary>
    /// 使用Biwen.AutoClassGen生成的Request
    /// </summary>
    [AutoGen("AutoGenRequest", "Biwen.QuickApi.DemoWeb.Apis")]
    public interface IAutoGenRequest : IPager, IQuery { }

    [FromBody]
    public partial class AutoGenRequest : BaseRequest<AutoGenRequest>
    {
    }

    [QuickApi("autogen", Verbs = Verb.POST)]
    [QuickApiSummary("自动生成的Request", "自动生成的接口")]
    [EndpointGroupName("group1")]
    public class AutoGenApi : BaseQuickApi<AutoGenRequest, IResultResponse>
    {
        public override async Task<IResultResponse> ExecuteAsync(AutoGenRequest request)
        {
            await Task.CompletedTask;
            return Results.Json(request).AsRspOfResult();
        }
    }
}