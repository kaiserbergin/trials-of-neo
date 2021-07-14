using System.Collections.Generic;
using System.Threading.Tasks;
using Neo4j.Driver;

namespace TrialsOfNeo
{
    public class QueryExecutor : IQueryExecutor
    {
        private readonly IDriver _driver;

        public QueryExecutor(IDriver driver)
        {
            _driver = driver;
        }

        public async Task<List<IRecord>> Read(string query)
        {
            var records = new List<IRecord>();
            using var session = _driver.AsyncSession();
            
            try
            {
                await session.ReadTransactionAsync(async tx =>
                {
                    var reader = await tx.RunAsync(query);

                    while (await reader.FetchAsync())
                    {
                        records.Add(reader.Current);
                    }
                });
            }
            finally
            {
                await session.CloseAsync();
            }
            
            return records;
        }
    }
}