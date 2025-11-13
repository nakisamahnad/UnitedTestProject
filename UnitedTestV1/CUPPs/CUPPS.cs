using Bogus;
using UnitedTestV1.Models;
using UnitedTestV1.MQTT;
using UnitedTestV1.PrinterModels;

namespace UnitedTestV1.CUPPs;

/// <summary>
/// The CUPPS class represents a client that communicates via MQTT protocol.
/// </summary>
public class CUPPS : ICUPPS, IDisposable
{
    public string Name { get; set; }
    public Guid Id { get; set; }    
    
    private readonly IMqttRoutingService _mqttRoutingService;
    
    private Task? _heartbeatTask;
    private CancellationTokenSource? _heartbeatCts;

    public List<Printers> Printers { get; set; } = new List<Printers>();


    

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
        
        Console.WriteLine("Starting Heartbeat...");
        StartHeartbeat();
    }


    public async Task SendHeartBeat()
    {
        var currentTime = DateTime.UtcNow;
        var last24h = currentTime.AddHours(-24);
        
        //send heartbeat message
        await _mqttRoutingService.SendHeartBeat(Id, new CU_HeartBeat()
        {
            AssignedPrinters = Printers.Count,
            cuId = Id,
            FaultyPrinters = Printers.Count(s=>s.IsFaulty),
            Last24hJobs = Printers.Sum(s=>s.Jobs.Count(a=>a.IsSuccess
            && a.SubmitDateTime >= last24h && a.SubmitDateTime <= currentTime)),
            LastCPU_Usage = new Faker().Random.Decimal(1, 100),
            LastRAM_Usage = new Faker().Random.Decimal(1, 100),
            Status = "OK",
            ts = new DateTimeOffset(currentTime).ToUnixTimeSeconds()
        });
    }
    
    
    
    public void StartHeartbeat()
    {
        if (_heartbeatTask is { IsCompleted: false })
            return; // already running

        _heartbeatCts = new CancellationTokenSource();
        var token = _heartbeatCts.Token;

        _heartbeatTask = Task.Run(async () =>
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        //Console.WriteLine("Send heart beat!");
                        await SendHeartBeat();
                    }
                    catch (Exception ex)
                    {
                        // log and continue
                        Console.WriteLine($"Heartbeat error: {ex.Message}");
                    }

                    await Task.Delay(TimeSpan.FromSeconds(5), token);
                }
            }
            catch (OperationCanceledException) { }
        }, token);
    }
    
    public void StopHeartbeat()
    {
        if (_heartbeatCts == null) return;
        _heartbeatCts.Cancel();
        try
        {
            _heartbeatTask?.Wait();
        }
        catch (AggregateException) { }
        finally
        {
            _heartbeatTask = null;
            _heartbeatCts.Dispose();
            _heartbeatCts = null;
        }
    }
    

    public Task<bool> MqttConnectionTest()
    {
        return _mqttRoutingService.MqttConnectionTest();
    }

    public Task RemoveOneMqttClient()
    {
        return _mqttRoutingService.RemoveOneMqttClient();
    }

    public Guid GetId  => Id;

    public void Dispose()
    {
        StopHeartbeat();
    }
}