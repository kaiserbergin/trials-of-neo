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
            const string bigCollectedQuery = @"
                MATCH (a:Person {name:'Tom Hanks'})
                OPTIONAL MATCH (a)-[r:ACTED_IN]->(m:Movie)
                OPTIONAL MATCH (m)<-[v:REVIEWED]-(rv:Person)
                OPTIONAL MATCH (rv)<-[f:FOLLOWS]-(fl:Person) 
                RETURN a, collect(r), collect(m), collect(v), collect(rv), collect(f), collect(fl)
                ";
            const string queryWithCalculatedField = "MATCH (a:Person {name:'Tom Hanks'}) RETURN a, 42";

            var oneToOneResults = await executor.Read(oneToOneQuery);
            var collectedResults = await executor.Read(collectedQuery);
            var bigCollectedResults = await executor.Read(bigCollectedQuery);
            var calculatedResults = await executor.Read(queryWithCalculatedField);

            var grapher = new Grapher(executor);

            var grapherOneToOneQuery = await grapher.ReadAs<ActorWithSingleMovie>(oneToOneQuery);
            var grapherCollected = await grapher.ReadAs<ActorWithMovies>(collectedQuery);
            var grapherBigCollected = await grapher.ReadAs<Person>(bigCollectedQuery);
        }

        // MATCH (a:Person {name:'Tom Hanks'})
        // OPTIONAL MATCH (a)-[r:ACTED_IN]->(m:Movie)
        // OPTIONAL MATCH (m)<-[v:REVIEWED]-(rv:Person)
        // OPTIONAL MATCH (rv)<-[f:FOLLOWS]-(fl:Person) 
        // RETURN a, collect(r), collect(m), collect(rv), collect(f), collect(fl)
    }
}