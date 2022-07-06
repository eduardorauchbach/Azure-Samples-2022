using Functions.SampleCosmos.Domain.Models;
using Functions.SampleCosmos.Domain.Repositories;
using Microsoft.Extensions.Logging;
using RauchTech.Extensions.Logging;
using RauchTech.Extensions.Logging.Services.Code;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Functions.SampleCosmos.Domain.Services.Code
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

        public async Task<ICollection<SampleModel>> GetAllAsync(string search)
        {
            ICollection<SampleModel> samples;

            try
            {
                _logger.LogCustom(LogLevel.Debug, message: CustomLogMessages.Begin);

                if (!string.IsNullOrWhiteSpace(search))
                {
                    samples = await _sampleRepository.GetAllAsync(x => x.Name.Contains(search)).ConfigureAwait(false);
                }
                else
                {
                    samples = await _sampleRepository.GetAllAsync().ConfigureAwait(false);
                }

                _logger.LogCustom(LogLevel.Debug, message: CustomLogMessages.Finish);
            }
            catch
            {
                throw;
            }

            return samples;
        }

        public async Task<SampleModel> GetByIdAsync(Guid sampleID, bool loadTokens = false)
        {
            SampleModel sample;

            try
            {
                _logger.LogCustom(LogLevel.Debug, message: CustomLogMessages.Begin);

                sample = await _sampleRepository.GetByIdAsync(sampleID);
                if (sample is null)
                {
                    throw new KeyNotFoundException(Constants.SampleNotFound + sampleID);
                }

                _logger.LogCustom(LogLevel.Debug, message: CustomLogMessages.Finish);
            }
            catch
            {
                throw;
            }

            return sample;
        }

        public async Task<SampleModel> CreateAsync(SampleModel sample)
        {
            try
            {
                _logger.LogCustom(LogLevel.Debug, message: CustomLogMessages.Begin);

                sample.Id = await GenerateNewID();

                sample.LastUpdate = DateTime.Now;

                await _sampleRepository.AddAsync(sample);

                _logger.LogCustom(LogLevel.Debug, message: CustomLogMessages.Finish);
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

        public async Task<SampleModel> UpdateAsync(SampleModel sample)
        {
            SampleModel sampleOld;

            try
            {
                _logger.LogCustom(LogLevel.Debug, message: CustomLogMessages.Begin);

                sampleOld = await GetByIdAsync(sample.Id); //Checking existance (caso não exista, vai estourar um erro)

                sample.LastUpdate = DateTime.Now;

                await _sampleRepository.UpdateAsync(sample);

                _logger.LogCustom(LogLevel.Debug, message: CustomLogMessages.Finish);
            }
            catch
            {
                throw;
            }

            return sample;
        }

        public async Task DeleteAsync(Guid sampleID)
        {
            SampleModel sample;

            try
            {
                _logger.LogCustom(LogLevel.Debug, message: CustomLogMessages.Begin);

                sample = await GetByIdAsync(sampleID);

                await _sampleRepository.DeleteAsync(sample);

                _logger.LogCustom(LogLevel.Debug, message: CustomLogMessages.Finish);
            }
            catch
            {
                throw;
            }
        }
    }
}
