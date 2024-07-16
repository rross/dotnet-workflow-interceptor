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
        LocalActivityOptions activityOptions = new () {
            // If you want to run this even when the workflow is cancelled
            // you need a different cancellation tokey

            CancellationToken = default(CancellationToken),
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
            Console.WriteLine("Executing workflow asynchronously...");
            var status = "failure";
            try 
            {
                var returnValue =  await base.ExecuteWorkflowAsync(input);
                status = "success";
                Console.WriteLine($"workflow async completed. Status is {status}",status);
                return returnValue;
            }   
            finally 
            {
                Console.WriteLine($"In finally block. Status is {status}", status);
                try 
                {
                    // call an activity to update the status
                    // This uses a LOCAL ACTIVITY to avoid signal loss
                    // Also, be sure that the local activity does an 
                    // upsert as it can be called multiple times
                    await Workflow.ExecuteLocalActivityAsync((MyActivities act) => act.SaveWorkflowStatus(status),activityOptions);
                    Console.WriteLine("Done updating status");
                }
                catch (Exception ex) 
                {
                    Console.WriteLine(ex);
                    Console.WriteLine(ex.StackTrace);
                }
            }
        }

    }
}