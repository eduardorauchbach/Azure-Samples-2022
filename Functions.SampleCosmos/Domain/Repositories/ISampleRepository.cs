using Functions.SampleCosmos.Domain.Models;
using RauchTech.Extensions.Data.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Functions.SampleCosmos.Domain.Repositories
{
    internal interface ISampleRepository : IBaseRepository<SampleModel>
    {
    }
}
