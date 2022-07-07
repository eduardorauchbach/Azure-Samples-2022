using Functions.SampleCosmos.Domain.Models;
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
