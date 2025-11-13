using UnitedTestV1.Models;

namespace UnitedTestV1.MQTT;

public interface IMqttRoutingService
{
    public Task<string> SendHandShake(Guid cuppsId, string computerName);
    
    public Task<string> SendHeartBeat(Guid cuppsId,CU_HeartBeat heartBeat);
    
    public Task<bool> MqttConnectionTest();
    
    public Task RemoveOneMqttClient();

    public Task<List<string>> GetActiveClientIds();

}