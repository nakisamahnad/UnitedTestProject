namespace UnitedTestV1.CUPPs;

public interface ICUPPS
{
    public Task RunCUPP();
    
    
    public Task<bool> MqttConnectionTest();
    
    
    public Task RemoveOneMqttClient();

    public Guid GetId { get; }
}