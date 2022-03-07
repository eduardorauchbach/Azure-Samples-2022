using Functions.Clients.Domain.Models;
using Functions.Clients.Domain.Repositories;
using Microsoft.Extensions.Logging;
using RauchTech.Extensions.Logging;
using RauchTech.Extensions.Logging.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Functions.Clients.Domain.Services.Code
{
    internal sealed class SampleService : ISampleService
    {
        #region Properties
        private readonly ICustomLog _logger;
        private readonly ISampleRepository _sampleRepository;
        #endregion

        #region Constructors
        public SampleService(
            ICustomLogFactory customLogFactory,
            ISampleRepository sampleRepository)
        {
            _logger = customLogFactory.CreateLogger<ISampleService>();
            _sampleRepository = sampleRepository;
        }
        #endregion

        public async Task<ICollection<SampleModel>> GetAllsamplesAsync()
        {
            ICollection<SampleModel> samples;

            try
            {
                _logger.LogCustom(LogLevel.Debug, message: CustomLog.Begin);

                samples = await _sampleRepository.GetAllAsync();

                _logger.LogCustom(LogLevel.Debug, message: CustomLog.Finish);
            }
            catch
            {
                throw;
            }

            return samples;
        }

        public async Task<SampleModel> GetByIdAsync(Guid limitID, bool loadTokens = false)
        {
            SampleModel limit;

            try
            {
                _logger.LogCustom(LogLevel.Debug, message: CustomLog.Begin);

                limit = await _sampleRepository.GetByIdAsync(limitID);
                if (limit is null)
                {
                    throw new KeyNotFoundException(Constants.SampleNotFound + limitID);
                }

                _logger.LogCustom(LogLevel.Debug, message: CustomLog.Finish);
            }
            catch
            {
                throw;
            }

            return limit;
        }

        public async Task<SampleModel> CreateLimit(SampleModel sample)
        {
            try
            {
                _logger.LogCustom(LogLevel.Debug, message: CustomLog.Begin);

                sample.Id = await GenerateNewID();

                sample.LastUpdate = DateTime.Now;

                await _sampleRepository.AddAsync(sample);

                _logger.LogCustom(LogLevel.Debug, message: CustomLog.Finish);
            }
            catch
            {
                throw;
            }

            return sample;


            #region Local

            async Task<Guid> GenerateNewID()
            {
                bool found;
                Guid id;

                do
                {
                    id = Guid.NewGuid();
                    found = await _sampleRepository.GetByIdAsync(id) != null;

                } while (found);

                return id;
            }

            #endregion
        }

        public async Task<SampleModel> UpdateLimit(SampleModel sample)
        {
            SampleModel limitOld;

            try
            {
                _logger.LogCustom(LogLevel.Debug, message: CustomLog.Begin);

                limitOld = await GetByIdAsync(sample.Id); //Checking existance (caso não exista, vai estourar um erro)

                sample.LastUpdate = DateTime.Now;

                await _sampleRepository.UpdateAsync(sample);

                _logger.LogCustom(LogLevel.Debug, message: CustomLog.Finish);
            }
            catch
            {
                throw;
            }

            return sample;
        }

        public async Task DeleteLimit(Guid limitID)
        {
            SampleModel limit;

            try
            {
                _logger.LogCustom(LogLevel.Debug, message: CustomLog.Begin);

                limit = await GetByIdAsync(limitID);

                await _sampleRepository.DeleteAsync(limit);

                _logger.LogCustom(LogLevel.Debug, message: CustomLog.Finish);
            }
            catch
            {
                throw;
            }
        }
    }
}
