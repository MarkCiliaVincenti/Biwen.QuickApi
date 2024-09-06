﻿// Licensed to the Biwen.QuickApi (net8.0) under one or more agreements.
// The Biwen.QuickApi (net8.0) licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi (net8.0) Author: vipwa Github: https://github.com/vipwan
// Modify Date: 2024-09-06 15:10:12 AuditModular.cs

using Biwen.QuickApi.Auditing.Internal;

namespace Biwen.QuickApi.Auditing;

[CoreModular]
internal class AuditModular : ModularBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddAuditHandler<ConsoleAuditHandler>();
        //注入审计代理
        services.TryAddSingleton(typeof(AuditProxyFactory<>));
    }

}
