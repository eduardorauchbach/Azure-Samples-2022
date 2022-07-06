using Functions.SampleCosmos.Domain.Models;
using Functions.SampleCosmos.Domain.Repositories;
using Microsoft.Extensions.Logging;
using RauchTech.Extensions.Logging;
using RauchTech.Extensions.Logging.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Functions.SampleCosmos.Domain.Services
{
    internal interface ISampleService
    {
        Task<ICollection<SampleModel>> GetAllAsync(string search);

        Task<SampleModel> GetByIdAsync(Guid sampleID, bool loadTokens = false);

        Task<SampleModel> CreateAsync(SampleModel sample);

        Task<SampleModel> UpdateAsync(SampleModel sample);

        Task DeleteAsync(Guid sampleID);
    }
}
