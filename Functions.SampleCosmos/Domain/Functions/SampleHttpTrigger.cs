using Functions.Common.Helpers;
using Functions.SampleCosmos.Domain.Functions.Helper;
using Functions.SampleCosmos.Domain.Models;
using Functions.SampleCosmos.Domain.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RauchTech.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Functions.SampleCosmos.Domain.Functions
{
    internal class SampleHttpTrigger
    {
        #region Properties
        private readonly ICustomLog _logger;
        private readonly ISampleService _sampleService;
        #endregion

        #region Constructors
        public SampleHttpTrigger(
            ICustomLogFactory customLogFactory,
            ISampleService sampleService)
        {
            _logger = customLogFactory.CreateLogger<SampleHttpTrigger>();
            _sampleService = sampleService;
        }
        #endregion

        //[ProducesResponseType(typeof(SampleModel), (int)HttpStatusCode.OK)]
        [FunctionName("HttpSampleGet")]
        [ServiceFilter(typeof(ActionFilters))]
        public async Task<IActionResult> Get(
                        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "samples/{id}")]
                        HttpRequest req, Guid id)
        {
            SampleModel sample;

            try
            {
                _logger.LogCustom(LogLevel.Information, message: CustomLogMessages.Begin);
                _logger.AddID("SampleID", id);

                sample = await _sampleService.GetByIdAsync(sampleID: id);

                _logger.LogCustom(LogLevel.Information, message: CustomLogMessages.Finish);
            }
            catch (Exception ex)
            {
                _logger.LogCustom(LogLevel.Error, exception: ex);
                return ex.GetExceptionResult();
            }

            return new OkObjectResult(sample);
        }

        //[ProducesResponseType(typeof(ICollection<SampleModel>), (int)HttpStatusCode.OK)]
        [FunctionName("HttpSampleGetAll")]
        [ServiceFilter(typeof(ActionFilters))]
        public async Task<IActionResult> GetAll(
                        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "samples")]
                        HttpRequest req)
        {
            ICollection<SampleModel> samples;

            string search;

            try
            {
                _logger.LogCustom(LogLevel.Information, message: CustomLogMessages.Begin);

                search = req.Query["search"];

                samples = await _sampleService.GetAllAsync(search: search);

                _logger.LogCustom(LogLevel.Information, message: CustomLogMessages.Finish);
            }
            catch (Exception ex)
            {
                _logger.LogCustom(LogLevel.Error, exception: ex);
                return ex.GetExceptionResult();
            }

            return new OkObjectResult(samples);
        }

        //[ProducesResponseType(typeof(SampleModel), (int)HttpStatusCode.OK)]
        [FunctionName("HttpSamplePost")]
        [ServiceFilter(typeof(ActionFilters))]
        public async Task<IActionResult> Post(
                        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "samples")]
                        //[RequestBodyType(typeof(SampleModel), "Comentário")]
                        HttpRequest req)
        {
            string bodyInfo = await req.ReadAsStringAsync();

            SampleModel sample;

            try
            {
                _logger.LogCustom(LogLevel.Information, message: CustomLogMessages.Begin);

                sample = JsonConvert.DeserializeObject<SampleModel>(bodyInfo);

                sample = await _sampleService.CreateAsync(sample: sample);

                _logger.AddID("SampleID", sample.Id);
                _logger.LogCustom(LogLevel.Information, message: CustomLogMessages.Finish);
            }
            catch (Exception ex)
            {
                _logger.LogCustom(LogLevel.Error, exception: ex);
                return ex.GetExceptionResult();
            }

            return new OkObjectResult(sample);
        }

        //[ProducesResponseType(typeof(SampleModel), (int)HttpStatusCode.OK)]
        [FunctionName("HttpSamplePut")]
        [ServiceFilter(typeof(ActionFilters))]
        public async Task<IActionResult> Put(
                        [HttpTrigger(AuthorizationLevel.Function, "put", Route = "samples/{sampleID}")]
                        //[RequestBodyType(typeof(SampleModel), "Comentário")]
                        HttpRequest req, Guid sampleID)
        {
            string bodyInfo = await req.ReadAsStringAsync();

            SampleModel sample;

            try
            {
                _logger.LogCustom(LogLevel.Information, message: CustomLogMessages.Begin);
                _logger.AddID("sampleID", sampleID);

                sample = JsonConvert.DeserializeObject<SampleModel>(bodyInfo);
                sample.Id = sampleID;

                sample = await _sampleService.UpdateAsync(sample: sample);

                _logger.LogCustom(LogLevel.Information, message: CustomLogMessages.Finish);
            }
            catch (Exception ex)
            {
                _logger.LogCustom(LogLevel.Error, exception: ex);
                return ex.GetExceptionResult();
            }

            return new OkObjectResult(sample);
        }

        //[ProducesResponseType((int)HttpStatusCode.OK)]
        [FunctionName("HttpSampleDelete")]
        [ServiceFilter(typeof(ActionFilters))]
        public async Task<IActionResult> Delete(
                        [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "samples/{sampleID}")]
                        HttpRequest req, Guid sampleID)
        {
            try
            {
                _logger.LogCustom(LogLevel.Information, message: CustomLogMessages.Begin);
                _logger.AddID("sampleID", sampleID);
                _logger.AddID("ComentarioID", sampleID);

                await _sampleService.DeleteAsync(sampleID: sampleID);

                _logger.LogCustom(LogLevel.Information, message: CustomLogMessages.Finish);
            }
            catch (Exception ex)
            {
                _logger.LogCustom(LogLevel.Error, exception: ex);
                return ex.GetExceptionResult();
            }

            return new OkResult();
        }
    }
}
