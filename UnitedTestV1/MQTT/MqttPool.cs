using MQTTnet;

namespace UnitedTestV1.MQTT;

/// <summary>
/// A pool for managing multiple MQTT clients.
/// </summary>
public class MqttPool
{
    private readonly List<IMqttClient> _clients;
    private int _currentIndex = 0;

    /// <summary>
    /// The default constructor of the class.
    /// </summary>
    public MqttPool()
    {
        _clients = new List<IMqttClient>();
    }


    /// <summary>
    /// Add a client to the pool.
    /// </summary>
    /// <param name="client">IMqttClient</param>
    public void AddClients(IMqttClient client)
    {
        _clients.Add(client);
    }
    
    // get a client from the pool in a round-robin fashion
    /// <summary>
    /// Get a client from the pool in a round-robin fashion.
    /// </summary>
    /// <returns>IMqttClient</returns>
    public IMqttClient GetClient()
    {
        if (_clients.Count == 0)
            throw new InvalidOperationException("No clients in the pool.");

        var client = _clients[_currentIndex];
        
        _currentIndex = (_currentIndex + 1) % _clients.Count;
        
        return client;
    }
    
    // remove a client from the pool
    /// <summary>
    /// Remove a client from the pool.
    /// </summary>
    /// <param name="client">IMqttClient</param>
    /// <returns></returns>
    public void RemoveClient(IMqttClient client)
    {
        _clients.Remove(client);
    }

    public List<IMqttClient> GetClients()
    {
        return _clients;
    }
    
    
    /// <summary>
    /// For getting the current index of the pool.
    /// </summary>
    /// <returns>int</returns>
    public int GetCurrentIndex()
    {
        return _currentIndex;
    }
}