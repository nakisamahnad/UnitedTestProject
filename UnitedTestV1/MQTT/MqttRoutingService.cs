using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Protocol;
using Newtonsoft.Json;
using UnitedTestV1.Models;

namespace UnitedTestV1.MQTT;

/// <summary>
/// The implementation of the MQTT routing service.
/// </summary>
public class MqttRoutingService : BackgroundService, IMqttRoutingService
{


    private string NsPrefix => $"abomis/" +
                               $"local-test";

    private string SharedGroup => "test-client";
    
    private readonly MqttPool _clientPool = new MqttPool();
    
    private readonly Guid _instanceId;
    
    private readonly int _maxClients;

    public MqttRoutingService(Guid instanceId,
        int maxClients)
    {
        _instanceId = instanceId;
        _maxClients = maxClients;
    }


    /// <summary>
    /// Override of the ExecuteAsync method from BackgroundService.
    /// To initialize and run the MQTT client.
    /// </summary>
    /// <param name="stoppingToken"></param>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        for (var counter = 0; counter < _maxClients; counter++)
        {
            var connector = await AwsIotCertConnector.ConnectAsync(_instanceId,
                counter);

            var client = connector.client;
            _clientPool.AddClients(client);

            client.ConnectedAsync += async e =>
            {
                
                //Console.Write("MQTT connected: {ResultCode} for {Id}");


                /*
                var sharedJobStatus =
                    $"{MqttKeys.Share}/{SharedGroup}/{NsPrefix}/{MqttKeys.PrintServiceClient}/+/{MqttKeys.Event}/{MqttKeys.Jobs}/+/{MqttKeys.Status}";
                var sharedPrinterStatus =
                    $"{MqttKeys.Share}/{SharedGroup}/{NsPrefix}/{MqttKeys.PrintServiceClient}/+/{MqttKeys.Event}/{MqttKeys.Printers}/+/{MqttKeys.Status}";
                var sharedHeartbeatStatus =
                    $"{MqttKeys.Share}/{SharedGroup}/{NsPrefix}/{MqttKeys.PrintServiceClient}/+/{MqttKeys.Event}/{MqttKeys.Heartbeat}";
                var sharedHandshake =
                    $"{MqttKeys.Share}/{SharedGroup}/{NsPrefix}/{MqttKeys.PrintServiceClient}/+/{MqttKeys.Event}/{MqttKeys.Handshake}";

                await client.SubscribeAsync(sharedJobStatus, MqttQualityOfServiceLevel.AtLeastOnce,
                    cancellationToken: stoppingToken);
                await client.SubscribeAsync(sharedPrinterStatus, MqttQualityOfServiceLevel.AtLeastOnce,
                    cancellationToken: stoppingToken);
                await client.SubscribeAsync(sharedHeartbeatStatus, MqttQualityOfServiceLevel.AtMostOnce,
                    cancellationToken: stoppingToken);
                await client.SubscribeAsync(sharedHandshake, MqttQualityOfServiceLevel.AtLeastOnce,
                    cancellationToken: stoppingToken);*/
                
                
                var sharedSavePrinters =
                    $"{MqttKeys.Share}/{_instanceId}/{NsPrefix}/{MqttKeys.PrintServiceClient}/{_instanceId}/{MqttKeys.Command}/{MqttKeys.Printers}/{MqttKeys.Remove}";
                
                await client.SubscribeAsync(sharedSavePrinters, MqttQualityOfServiceLevel.AtLeastOnce,
                    cancellationToken: stoppingToken);
            };

            client.ApplicationMessageReceivedAsync += async e =>
            {
                try
                {
                    //using var scope = _serviceScopeFactory.CreateScope();

                    var topic = e.ApplicationMessage.Topic ?? string.Empty;
                    var payload = e.ApplicationMessage.ConvertPayloadToString();
                    var parts = topic.Split('/', StringSplitOptions.RemoveEmptyEntries);

                    
                    Console.WriteLine("Received MQTT message on topic: {Topic} with {Payload}", topic, payload);
                    
                }
                catch (Exception ex)
                {
                    Console.WriteLine( "Error handling MQTT message");
                    //_log.LogError(ex,);
                }
            };

            client.DisconnectedAsync += async e =>
            {
                Console.Write("MQTT disconnected: {Reason} {Message}", e.Reason, e.Exception?.Message);
                // simple backoff reconnect
                var delay = TimeSpan.FromSeconds(2);
                while (!stoppingToken.IsCancellationRequested && client is { IsConnected: false })
                {
                    try
                    {
                        await Task.Delay(delay, stoppingToken);
                        await client!.ReconnectAsync();
                        delay = TimeSpan.FromSeconds(2); // reset on success
                    }
                    catch (OperationCanceledException)
                    {
                    }
                    catch (Exception ex)
                    {
                        //_log.LogWarning(ex, "Reconnect failed; retrying");
                        delay = TimeSpan.FromSeconds(Math.Min(delay.TotalSeconds * 2, 30));
                    }
                }
            };

            // connect the client
            await client.ConnectAsync(connector.options, stoppingToken);
        }

        // keep hosted service alive
        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            /* normal shutdown */
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<string> SendHandShake(Guid cuppsId, string computerName)
    {
        var client = _clientPool.GetClient();

        if (client is not { IsConnected: true })
            throw new InvalidOperationException("MQTT client is not connected.");
        
        var corr = Guid.NewGuid().ToString("N");
        
        
        string topic = "";

        topic =
            $"{NsPrefix}/{MqttKeys.PrintServiceClient}/{cuppsId}/{MqttKeys.Event}/{MqttKeys.Handshake}";

        var model = new CU_Handshake()
        {
            ComputerName = computerName,
            IsNetwork = true,
            Timestamp = DateTime.UtcNow,
            WorkStationId = cuppsId
        };

        var msg = new MqttApplicationMessageBuilder()
            .WithTopic(topic)
            .WithPayload(JsonConvert.SerializeObject(model))
            .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
            .WithRetainFlag(false)
            .Build();

        var result = await client.PublishAsync(msg, CancellationToken.None);


        return corr;
    }

    public async Task<string> SendHeartBeat(Guid cuppsId,CU_HeartBeat heartBeat)
    {
        var client = _clientPool.GetClient();

        if (client is not { IsConnected: true })
            throw new InvalidOperationException("MQTT client is not connected.");
        
        var corr = Guid.NewGuid().ToString("N");
        
        
        string topic = "";

        topic =
            $"{NsPrefix}/{MqttKeys.PrintServiceClient}/{cuppsId}/{MqttKeys.Event}/{MqttKeys.Heartbeat}";
        
        
        var msg = new MqttApplicationMessageBuilder()
            .WithTopic(topic)
            .WithPayload(JsonConvert.SerializeObject(heartBeat))
            .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
            .WithRetainFlag(false)
            .Build();

        var result = await client.PublishAsync(msg, CancellationToken.None);


        return corr;
    }

    public Task<bool> MqttConnectionTest()
    {
        var allConnected = _clientPool.GetClients();
        return Task.FromResult(allConnected.All(c => c.IsConnected));
    }

    
    public async Task RemoveOneMqttClient()
    {
        var client = _clientPool.GetClient();

        await client?.DisconnectAsync()!;
        
        client.Dispose();
        
        _clientPool.RemoveClient(client);
    }
}