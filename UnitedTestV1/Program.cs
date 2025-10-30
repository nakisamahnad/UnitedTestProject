// See https://aka.ms/new-console-template for more information

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using UnitedTestV1.CUPPs;
using UnitedTestV1.MQTT;

Console.WriteLine("Hello, World!");

Guid cuppsId = Guid.Parse("04d9a73d-3506-4164-b402-0dc4ee22ff46");

var host1 = CreateMqttHostAsync(cuppsId);

var task = host1.RunAsync();

var cupps = host1.Services.GetRequiredService<ICUPPS>();

while (!await cupps.MqttConnectionTest())
{
   Thread.Sleep(1000); 
}

Console.WriteLine("MQTT Connection Test Successful.");

await cupps.RunCUPP();



Console.ReadKey();

Console.WriteLine("Let's remove one MQTT client now.");

await cupps.RemoveOneMqttClient();
Console.ReadKey();



static IHost CreateMqttHostAsync(Guid instanceId)
{

    return Host.CreateDefaultBuilder()
        .ConfigureServices((context, services) =>
        {
            
            services.AddSingleton<MqttRoutingService>(s 
                => new MqttRoutingService(instanceId)); // concrete singleton
            services.AddSingleton<IMqttRoutingService>(sp => // interface maps to the same instance
                sp.GetRequiredService<MqttRoutingService>());
            services.AddHostedService(sp => // host it as a background service
                sp.GetRequiredService<MqttRoutingService>());

            services.AddSingleton<ICUPPS, CUPPS>(s => 
                new CUPPS(s.GetRequiredService<IMqttRoutingService>(),
                instanceId));

        })
        .Build();
}