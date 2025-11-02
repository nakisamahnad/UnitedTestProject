namespace UnitedTestV1.Models;

public class CU_HeartBeat
{
    /// <summary>
    /// Timestamp of the heartbeat in Unix time seconds
    /// </summary>
    public long ts { get; set; } 
    
    /// <summary>
    /// The unique identifier of the workstation sending the heartbeat
    /// </summary>
    public Guid cuId { get; set; }
    
    /// <summary>
    /// Last reported CPU usage percentage of the workstation
    /// </summary>
    /// <example>10</example>
    public decimal? LastCPU_Usage { get; set; }

    /// <summary>
    /// Last reported RAM usage percentage of the workstation
    /// </summary>
    /// <example>10</example>
    public decimal? LastRAM_Usage{ get; set; }

    /// <summary>
    /// Number of printers currently assigned to the workstation
    /// </summary>
    /// <example>100</example>
    public int? AssignedPrinters { get; set; }

    /// <summary>
    /// Number of faulty printers currently assigned to the workstation
    /// </summary>
    /// <example>2</example>
    public int? FaultyPrinters { get; set; }

    /// <summary>
    /// Number of print jobs processed by the workstation in the last 24 hours
    /// </summary>
    public int? Last24hJobs { get; set; }

    /// <summary>
    /// The last status message from the workstation
    /// </summary>
    public string? Status { get; set; }
}