namespace UnitedTestV1.Models;

public class CU_Handshake
{
    /// <summary>
    /// Id of the workstation
    /// </summary>
    /// <remarks>
    /// Either it has submitted before or it is a new workstation.
    /// </remarks>
    public Guid WorkStationId  { get; set; }

    /// <summary>
    /// mark if the computer is network or not.
    /// </summary>
    public bool IsNetwork { get; set; }
    
    
    /// <summary>
    /// OPTIONAL - Name of the Computer
    /// </summary>
    public string? ComputerName { get; set; }
    
    /// <summary>
    /// Time of the status report (on the CUPPs server)
    /// in UTC
    /// </summary>
    public DateTime Timestamp { get; set; }
}