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
        Task<ICollection<SampleModel>> GetAllsamplesAsync();

        Task<SampleModel> GetByIdAsync(Guid limitID, bool loadTokens = false);

        Task<SampleModel> CreateLimit(SampleModel sample);

        Task<SampleModel> UpdateLimit(SampleModel sample);

        Task DeleteLimit(Guid limitID);
    }
}
