using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Configuration;
using MQTTnet;

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
        
        if(pfxPath == null || pfxPassword == null || crtPath == null)
        {
            throw new ArgumentNullException("Environment variables for MQTT certificates are not set.");
        }

        if (!File.Exists(pfxPath) || !File.Exists(crtPath))
        {
            throw new FileNotFoundException("Mqtt certificate files not found.");
        }

        var deviceCert = new X509Certificate2(
            pfxPath,
            pfxPassword,
            X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.Exportable);
        
        var rootCa = new X509Certificate2(crtPath);
        
        
        var mqttFactory = new MqttClientFactory();

        var client = mqttFactory.CreateMqttClient();

        var tlsOptions = new MqttClientTlsOptions
        {
            UseTls = true,
            SslProtocol = SslProtocols.Tls12,
            ClientCertificatesProvider = new DefaultMqttCertificatesProvider(new List<X509Certificate> { deviceCert }),
            CertificateValidationHandler = ctx =>
            {
                try
                {
                    using var chain = new X509Chain();
                    chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
                    // Allow system roots; add extra root if present
                    chain.ChainPolicy.ExtraStore.Add(rootCa);
                    chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllowUnknownCertificateAuthority;
                    

                    var serverCert2 = ctx.Certificate as X509Certificate2 ?? new X509Certificate2(ctx.Certificate);
                    var ok = chain.Build(serverCert2);

                    // If the only error is UntrustedRoot but it chains to our provided root, accept
                    if (!ok &&
                        chain.ChainStatus.Length == 1 &&
                        chain.ChainStatus[0].Status == X509ChainStatusFlags.UntrustedRoot &&
                        string.Equals(chain.ChainElements[^1].Certificate.Thumbprint, rootCa.Thumbprint, System.StringComparison.OrdinalIgnoreCase))
                    {
                        ok = true;
                    }

                    if (!ok)
                    {
                        // Log chain errors for diagnostics
                        foreach (var s in chain.ChainStatus)
                            Console.WriteLine($"[TLS] {s.Status}: {s.StatusInformation}");
                    }
                    return ok;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[TLS] Validation exception: {ex.Message}");
                    return false;
                }
            }
        };


        var clientId = Create(cuppsId.ToString(), channelId);
        var options = new MqttClientOptionsBuilder()
            .WithClientId(clientId) // FOR NOW we can only use basicPubSub as client id
            .WithTcpServer("ahxakebqwf8xf-ats.iot.us-east-1.amazonaws.com", 8883)
            .WithProtocolVersion(MQTTnet.Formatter.MqttProtocolVersion.V500)
            .WithCleanSession(false)
            .WithKeepAlivePeriod(TimeSpan.FromSeconds(60))
            .WithReceiveMaximum(1000)
            .WithTlsOptions(tlsOptions)
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