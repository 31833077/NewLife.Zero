﻿using NewLife;
using NewLife.Log;
using NewLife.Remoting;
using NewLife.Serialization;
using Stardust;
using Stardust.Registry;

namespace Zero.WebApi.Services;

/// <summary>
/// 星尘注册中心用法，消费其它应用提供的服务
/// </summary>
public class MyHostedService : IHostedService
{
    private readonly IRegistry _registry;
    private readonly StarFactory _factory;
    private ApiHttpClient _client;

    public MyHostedService(IRegistry registry, StarFactory factory)
    {
        _registry = registry;
        _factory = factory;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        //// 从注册中心获取地址
        //var services = await _registry.ResolveAsync("Zero.WebApi");
        //XTrace.WriteLine("Zero.WebApi服务信息：{0}", services.ToJson(true));

        _ = Task.Factory.StartNew(async () =>
        {
            // 异步执行，延迟消费，等外部先完成注册
            await Task.Delay(3_000);

            // 创建指定服务的客户端，它的服务端地址绑定注册中心，自动更新
            _client = await _factory.CreateForServiceAsync("Zero.WebApi") as ApiHttpClient;
            XTrace.WriteLine("Zero.WebApi服务地址：{0}", _client.Services.Select(e => e.Address).Join());

            // 尝试调用接口
            var rs = await _client?.GetAsync<Object>("api/info", new { state = "NewLife1234" });
            XTrace.WriteLine("api接口信息：{0}", rs.ToJson(true));
        });

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _client.TryDispose();

        return Task.CompletedTask;
    }
}