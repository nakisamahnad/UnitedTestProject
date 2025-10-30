using Bogus;
using UnitedTestV1.MQTT;

namespace UnitedTestV1.CUPPs;

public class CUPPS : ICUPPS
{
    public string Name { get; set; }
    public Guid Id { get; set; }    
    
    private readonly IMqttRoutingService _mqttRoutingService;
    

    public CUPPS(IMqttRoutingService mqttRoutingService,
        Guid cuppsId)
    {
        _mqttRoutingService = mqttRoutingService;

        Id = cuppsId;
        
        // create random Name
        Name = new Faker().Name.FirstName();

    }

    public async Task RunCUPP()
    {
        Console.WriteLine($"Running cupps for {Name}");
        
        Console.WriteLine("Sending HandShake...");
        await _mqttRoutingService.SendHandShake(Id, Name);
    }

    public Task<bool> MqttConnectionTest()
    {
        return _mqttRoutingService.MqttConnectionTest();
    }

    public Task RemoveOneMqttClient()
    {
        return _mqttRoutingService.RemoveOneMqttClient();
    }
}