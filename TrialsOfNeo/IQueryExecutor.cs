using System.Collections.Generic;
using System.Threading.Tasks;
using Neo4j.Driver;

namespace TrialsOfNeo
{
    public interface IQueryExecutor
    {
        Task<List<IRecord>> Read(string query);
    }
}