using System;
using System.Threading.Tasks;
using Serilog;
using Serilog.Core;

namespace TrialsOfNeo
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var settings = new NeoDriverConfigurationSettings()
            {
                Url = "bolt://localhost:7687",
                Username = "neo4j",
                Password = "password",
                IsDebugLoggingEnabled = true,
                IsTraceLoggingEnabled = true
            };
            
            var logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();

            using var driverProvider = new DriverProvider(settings, new NeoLogger(logger, settings));
            var executor = new QueryExecutor(driverProvider.Driver);

            const string oneToOneQuery = "MATCH (a:Person {name:'Tom Hanks'})-[r:ACTED_IN]->(m:Movie) RETURN a, r, m LIMIT 5";
            const string collectedQuery = "MATCH (a:Person {name:'Tom Hanks'})-[r:ACTED_IN]->(m:Movie) RETURN a, collect(r), collect(m)";

            var oneToOneResults = await executor.Read(oneToOneQuery);
            var collectedResults = await executor.Read(collectedQuery);

            var grapher = new Grapher(executor);
            
            grapher.PopulateNodes(collectedResults);

            var grapherCollected = await grapher.ReadAs<Actor>(collectedQuery);
            var grapherOneToOneQuery = await grapher.ReadAs<Actor>(oneToOneQuery);
        }
    }
}