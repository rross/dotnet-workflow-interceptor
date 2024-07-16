using DotNetTemplate;
using Microsoft.Extensions.Logging;
using Temporalio.Api.Common.V1;
using Temporalio.Worker.Interceptors;
using Temporalio.Workflows;

public class MyWorkflowInterceptor : IWorkerInterceptor
{
    public Action<string, IReadOnlyDictionary<string, Payload>?>? OnInbound { get; set; }

    public WorkflowInboundInterceptor InterceptWorkflow(WorkflowInboundInterceptor nextInterceptor) => 
        new WorkflowInbound(this, nextInterceptor);

    private sealed class WorkflowInbound : WorkflowInboundInterceptor
    {
        ActivityOptions activityOptions = new () {
            StartToCloseTimeout = TimeSpan.FromSeconds(5),
            RetryPolicy = new() {
                InitialInterval = TimeSpan.FromSeconds(1),
                BackoffCoefficient = 2,
                MaximumInterval = TimeSpan.FromSeconds(10)
                }
         };

        private readonly MyWorkflowInterceptor root;

        internal WorkflowInbound(MyWorkflowInterceptor root, WorkflowInboundInterceptor next) 
            : base(next) => this.root = root;

        public override void Init(WorkflowOutboundInterceptor outbound)
        {
            // if necessary, you can override this by instantiating an Outbound interceptor class
            base.Init(outbound);
        }

        public override async Task<object?> ExecuteWorkflowAsync(ExecuteWorkflowInput input)
        {
            // Workflow.Logger.LogInformation("Executing workflow asynchronously...");
            Console.WriteLine("execute workflow async...");
            var status = "failure";
            try 
            {
                var returnValue =  await base.ExecuteWorkflowAsync(input).ConfigureAwait(false);
                status = "success";
                // Workflow.Logger.LogInformation("workflow async completed. Status is {status}", status);
                Console.WriteLine("async completed");
                return returnValue;
            }   
            finally 
            {
                // Workflow.Logger.LogInformation("In finally block. Status is {status}", status);
                Console.WriteLine("in finally");
                try 
                {
                    // call an activity to update the status
                    await Workflow.ExecuteActivityAsync((MyActivities act) => act.SaveWorkflowStatus(status),activityOptions);
                    // Workflow.Logger.LogInformation("Done updating status");
                    Console.WriteLine("in try block");
                }
                catch (Exception ex) 
                {
                    // Workflow.Logger.LogError("An error occurred attempting to update the status {ex}", ex);
                    Console.WriteLine(ex);
                }
            }
        }

    }
}