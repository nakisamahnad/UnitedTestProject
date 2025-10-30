namespace UnitedTestV1.MQTT;

public interface IMqttRoutingService
{
    public Task<string> SendHandShake(Guid cuppsId, string computerName);
    
    public Task<bool> MqttConnectionTest();
    
    public Task RemoveOneMqttClient();
}