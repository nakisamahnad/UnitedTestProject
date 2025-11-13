using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using UnitedTestV1.CUPPs;
using UnitedTestV1.MQTT;

namespace UnitedTestV1;

public class Simulator
{
    public int NoCupps { get; set; }

    public List<ICUPPS> CuppsPools { get; set; } = new List<ICUPPS>();

    public void Run()
    {
        Console.WriteLine("Start the simulation and test environment...");

        Console.WriteLine("How many CUPPS to simulate? (default 1): \n");
        var input = Console.ReadLine();
        if (int.TryParse(input, out int noCupps))
        {
            NoCupps = noCupps;
            Console.WriteLine($"Simulating {NoCupps} CUPPS instances...");
        }
        else
        {
            Console.WriteLine("Invalid input. Using default of 1 CUPPS.");
            NoCupps = 1;
        }
        
        // loop to create multiple CUPPS instances
        for (var i = 0; i < NoCupps; i++)
        {
            var cupps = CreateNewCupps();
            Console.WriteLine($"CUPPS instance {i + 1} created with ID: {cupps.GetId}");
        }

        Console.WriteLine("Waiting for all CUPPS to establish MQTT connections...");
        foreach (var cupps in CuppsPools)
        {
            while (!cupps.MqttConnectionTest().Result)
            {
                System.Threading.Thread.Sleep(1000);
            }
            
        }
        
        Console.WriteLine("All CUPPS MQTT Connection Test Successful.");
    }

    public void RunAllCupps()
    {
        CuppsPools.ForEach(cupps => cupps.RunCUPP().Wait());
    }
    
    private ICUPPS CreateNewCupps(Guid? cuppsId = null)
    {
        cuppsId ??= Guid.NewGuid();
        
        // create a random number between 5-30 for mqtt clients
        var random = new Random();
        var maxMqttClients = random.Next(1, 5);
        
        var host = CreateMqttHostAsync(cuppsId.Value,maxMqttClients);
        var task = host.RunAsync();
        
        
        
        var cupps = host.Services.GetRequiredService<ICUPPS>();
        
        CuppsPools.Add(cupps);
        
        return cupps;
    }
    
    
    private IHost CreateMqttHostAsync(Guid instanceId,
        int mqttClients = 10)
    {

        return Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
            
                services.AddSingleton<MqttRoutingService>(s 
                    => new MqttRoutingService(instanceId,mqttClients)); // concrete singleton
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
}