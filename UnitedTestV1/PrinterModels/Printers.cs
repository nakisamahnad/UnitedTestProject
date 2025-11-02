namespace UnitedTestV1.PrinterModels;

public class Printers
{
   /// <summary>
   /// Id of the printer 
   /// </summary>
   public string Id { get; set; }
   
   /// <summary>
   /// The name of the printer status.
   /// </summary>
   /// <remarks>
   /// it can be null since the printer can be added without a status and the status will be assigned later to it.
   /// </remarks>
   public string? PrinterStatusName { get; set; }

   /// <summary>
   /// List of the jobs assigned to the printer.
   /// </summary>
   public List<Job> Jobs { get; set; } = new List<Job>();
   
   
   public bool IsFaulty => !ValidPrinterStatusNames.Contains(PrinterStatusName);


   public static string[] ValidPrinterStatusNames =
   [
       "init",
       "ready",
       "printing",
   ];
}