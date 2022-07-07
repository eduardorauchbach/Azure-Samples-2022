using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using RauchTech.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Functions.SampleCosmos.Domain.Functions
{
    internal class SampleQueueTrigger
    {
        #region Properties
        private readonly ICustomLog _logger;
        #endregion

        #region Constructors
        public SampleQueueTrigger(
            ICustomLogFactory customLogFactory)
        {
            _logger = customLogFactory.CreateLogger<SampleQueueTrigger>();
        }
        #endregion

        [FunctionName("Sample Execution")]
        public async Task OnSampleKeyReceived(
            [QueueTrigger("%Queue_Start%", Connection = "StorageConnectionString")]
            string id,

            [Queue("%Queue_Exit%", Connection = "StorageConnectionString")]
            IAsyncCollector<string> exitQueue)
        {
            try
            {
                _logger.LogCustom(LogLevel.Information, message: CustomLogMessages.Begin);
                _logger.AddID("Key", id);

                //Some Code

                await exitQueue.AddAsync(id).ConfigureAwait(false);

                _logger.LogCustom(LogLevel.Information, message: CustomLogMessages.Begin);
            }
            catch (Exception ex)
            {
                _logger.LogCustom(LogLevel.Error, exception: ex);
                throw;
            }
        }
    }
}
