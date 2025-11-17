using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Configuration;
using MQTTnet;
using MQTTnet.Protocol;
using Newtonsoft.Json;
using UnitedTestV1.Models;

namespace UnitedTestV1.MQTT;

public class AwsIotCertConnector
{
    
    private static int _seq;

    /// <summary>
    /// A method for connecting to AWS IoT using X.509 certificates.
    /// </summary>
    /// <param name="configuration">IConfiguration: required for reading the configuration</param>
    /// <param name="channelId"></param>
    /// <exception cref="ArgumentNullException">will be thrown in case the envs haven't been set</exception>
    /// <exception cref="FileNotFoundException">FileNotFoundException: will be thrown in case the certifications
    /// haven't set</exception>
    /// <returns>IMqttClient</returns>
    public static async Task<(IMqttClient client, MqttClientOptions options)> ConnectAsync(Guid cuppsId,
        int channelId)
    {
        var pfxPath = "C:\\Users\\ParsaMann\\RiderProjects\\UnitedTestProject\\UnitedTestV1\\MQTT\\device.pfx";
        var crtPath = "C:\\Users\\ParsaMann\\RiderProjects\\UnitedTestProject\\UnitedTestV1\\MQTT\\root-CA.crt";
        var pfxPassword = "123";
        
       
        
        var mqttFactory = new MqttClientFactory();

        var client = mqttFactory.CreateMqttClient();

        var tlsOptions = new MqttClientTlsOptions
        {
            UseTls = true,
            SslProtocol = SslProtocols.Tls12
            //ClientCertificatesProvider = new DefaultMqttCertificatesProvider(new List<X509Certificate> { deviceCert }),
            
        };
        
        var clientId = Create(cuppsId.ToString(), channelId);
        
        
        var topicWill =
            $"{MqttRoutingService.NsPrefix}/{MqttKeys.PrintServiceClient}/{cuppsId}/{MqttKeys.WILL}";
        
        var willMessage = new MqttApplicationMessageBuilder()
            .WithTopic(topicWill)   // LWT topic
            .WithPayload(JsonConvert.SerializeObject(new
            {
                ClientId = clientId,
                Status = "offline"
            }))
            .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
            .WithRetainFlag(false) 
            .Build();


        var options = new MqttClientOptionsBuilder()
            .WithClientId(clientId) // FOR NOW we can only use basicPubSub as client id
            .WithTcpServer("mr-connection-qzscuk4gmj4.messaging.solace.cloud", 8883)
            .WithProtocolVersion(MQTTnet.Formatter.MqttProtocolVersion.V500)
            .WithCleanSession(false)
            .WithKeepAlivePeriod(TimeSpan.FromSeconds(60))
            .WithReceiveMaximum(10000)
            .WithTlsOptions(tlsOptions)
            .WithCredentials(username: "solace-cloud-client", password: "g7rhc0bkuu6nijf7pp20d5dbll")
            .WithWillTopic(topicWill)
            .WithWillPayload(JsonConvert.SerializeObject(new CU_Will
            {
                ClientId = clientId
            }))
            .WithWillRetain(false)
            .WithWillQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
            .Build();



        //client.ConnectedAsync += onConnected;

        //client.DisconnectedAsync += onDisconnected;

        //client.

        //await client.ConnectAsync(options);
        return (client, options);
    }
    
    public static string Create(string serviceGuid, int channelIndex /* 0..99 */)
    { 
        string ch = channelIndex.ToString("D2");             // 00..99
        int pid = Environment.ProcessId;
        string seqHex = Interlocked.Increment(ref _seq).ToString("X"); // A, B, C...
        string rand6 = Rand6();

        return $"CUPPS_{serviceGuid}_ch{ch}_pid{pid}_s{seqHex}_r{rand6}";
    }

    private static string Rand6()
    {
        Span<byte> buf = stackalloc byte[4]; // 32 bits -> ~6 base64url chars
        RandomNumberGenerator.Fill(buf);
        string b64 = Convert.ToBase64String(buf).TrimEnd('=').Replace('+','-').Replace('/','_');
        return b64.Length >= 6 ? b64[..6] : b64.PadRight(6, '0');
    }
}