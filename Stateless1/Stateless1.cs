using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Serilog.Core.Enrichers;

using Microsoft.Extensions.Logging;
using Serilog;

namespace Stateless1
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class Stateless1 : StatelessService
    {
        private Microsoft.Extensions.Logging.ILogger _logger;
        public Stateless1(StatelessServiceContext context, Serilog.ILogger log)
            : base(context)
        {
            //Enrich Serilog with service information
            //See https://github.com/MicrosoftDocs/azure-docs/blob/master/articles/service-fabric/service-fabric-diagnostics-event-generation-app.md
            PropertyEnricher[] properties = new PropertyEnricher[]
            {
                   new PropertyEnricher("ServiceTypeName", context.ServiceTypeName),
                   new PropertyEnricher("ServiceName", context.ServiceName),
                   new PropertyEnricher("PartitionId", context.PartitionId),
                   new PropertyEnricher("InstanceId", context.ReplicaOrInstanceId),
                   new PropertyEnricher("TraceId", context.TraceId),                   
            };

            log.ForContext(properties);

            _logger = new LoggerFactory().AddSerilog(log.ForContext(properties)).CreateLogger<Stateless1>();
        }

        /// <summary>
        /// Optional override to create listeners (e.g., TCP, HTTP) for this service replica to handle client or user requests.
        /// </summary>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new ServiceInstanceListener[0];
        }

        /// <summary>
        /// This is the main entry point for your service instance.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service instance.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            // TODO: Replace the following sample code with your own logic 
            //       or remove this RunAsync override if it's not needed in your service.

            long iterations = 0;

            while (true)
            {
                _logger.LogTrace(iterations.ToString());
                cancellationToken.ThrowIfCancellationRequested();

                try
                {                    
                    if(++iterations%10==0)
                    {
                        throw new IndexOutOfRangeException("Current iterations: " + iterations);
                    }
                    //Example of using Event Source with structured events
                    ServiceEventSource.Current.ServiceMessage(this.Context, "Structured EventSource:Working-{0}", iterations);

                    //Example of using Event Source generically
                    ServiceEventSource.Current.Debug(string.Format("Generic EventSource:Working-{0}", iterations));

                    //Example of using Serilog with ILogger                    
                    _logger.LogInformation("Serilog:Working-{0}", iterations);
                }
                catch(System.Exception oops)
                {

                    //Example of using Event Source generically
                    ServiceEventSource.Current.Error(oops.StackTrace, string.Format("Generic EventSource:Iterations-{0}", iterations));

                    //Example of using Serilog
                    _logger.LogError(101, oops, "Serilog:Iterations-{0}", iterations);                    
                }
                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }
    }
}
