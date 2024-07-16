using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Temporalio.Activities;

namespace DotNetTemplate;

public record EchoInput(String val);
public record EchoOutput(String result);

public class MyActivities
{
    private int number; 

    public MyActivities(int number) 
    {
        this.number = number;
    }

    [Activity]
    public String Echo1(String input) 
    {
        ActivityExecutionContext.Current.Logger.LogInformation("Echo1 activity started, input = {input}",input);
     
        Thread.Sleep(TimeSpan.FromSeconds(1));
        
        return input;
    }
     
    [Activity]
    public String Echo2(String input) 
    {
        ActivityExecutionContext.Current.Logger.LogInformation("Echo2 activity started, input = {input}",input);
     
        Thread.Sleep(TimeSpan.FromSeconds(1));
        
        return input;
    }    

    [Activity]
    public String Echo3(String input) 
    {
        ActivityExecutionContext.Current.Logger.LogInformation("Echo3 activity started, input = {input}",input);
     
        Thread.Sleep(TimeSpan.FromSeconds(1));
        
        return input;
    }  

    [Activity]
    public EchoOutput Echo4(EchoInput input)
    {
        ActivityExecutionContext.Current.Logger.LogInformation("Echo3 activity started, input = {input}",input);
     
        Thread.Sleep(TimeSpan.FromSeconds(1));

        String result = "";
        for(int i=0;i<number;i++) 
        {
            result += $"{i} ";
        }
        
        return new EchoOutput(result);
    }

    [Activity]
    public void SaveWorkflowStatus(String status)
    {
        ActivityExecutionContext.Current.Logger.LogInformation("SaveWorkflowStatus activity started, status = {status}",status);
        // Do something with the status (e.g. persist to DB)
        // Be sure that this activity can handle being called multiple times
        // (Replay scenarios)
        // So if using a datasource, use an upsert 
        Console.WriteLine($"Saving the status {status} to a database");
    }
}
