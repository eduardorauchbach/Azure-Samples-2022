using Functions.Clients.Domain.Models;
using RauchTech.Extensions.Data.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Functions.Clients.Domain.Repositories
{
    internal interface ISampleRepository : IBaseRepository<SampleModel>
    {
    }
}
