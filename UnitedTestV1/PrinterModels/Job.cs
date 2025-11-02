namespace UnitedTestV1.PrinterModels;

public class Job
{
    /// <summary>
    /// Id of the job entity.
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// The submission date and time of the job.
    /// </summary>
    public DateTime SubmitDateTime { get; set; }
    
    /// <summary>
    /// Status of the job
    /// </summary>
    /// sqlserver-ex-backup-restore
    public string Status { get; set; }
    
    /// <summary>
    /// Define if the job was successful or not
    /// </summary>
   public bool IsSuccess => !SuccessStatuses.Contains(Status);
    
    
    
    public static string[] SuccessStatuses = new[]
    {
        "Completed"
    };
}