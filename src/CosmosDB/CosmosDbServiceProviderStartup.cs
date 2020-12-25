using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.DependencyInjection.Extensions;

[assembly: Microsoft.Azure.WebJobs.Hosting.WebJobsStartup(typeof(Microsoft.Azure.Workflows.ServiceProvider.Extensions.CosmosDB.CosmosDbServiceProviderStartup))]
namespace Microsoft.Azure.Workflows.ServiceProvider.Extensions.CosmosDB
{
    /// <summary>
    /// This is a start up function, the discovery of this extension is based upon IWebJobsStartup implementation. 
    /// In the function log file you should be able to see the log "Loading startup extension 'CosmosDbServiceProvider'"
    /// </summary>
    public class CosmosDbServiceProviderStartup : IWebJobsStartup
    {
        /// <summary>
        /// The Configure method is invoked as initialization of the extension.
        /// </summary>
        /// <param name="builder"></param>
        public void Configure(IWebJobsBuilder builder)
        {
            builder.AddExtension<CosmosDbServiceProvider>();
            builder.Services.TryAddSingleton<CosmosDbServiceOperationProvider>();
        }
    }
}
